using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Events;
using BuyTime_Domain.Entities;
using MediatR;
using ErrorOr;

namespace BuyTime_Infrastructure.Services;

public class BookingService(
    ITelegramService telegramService,
    IUnitOfWork unitOfWork,
    IMediator mediator) : IBookingService
{
    public async Task<Guid> CreateBookingAsync(Guid userId, Guid teacherId, 
        Guid timeslotId, string message, string status)
    {
        var user = await unitOfWork.Student.GetByIdAsync(userId);
        var teacher = await unitOfWork.Teacher.GetByIdAsync(teacherId);
        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(timeslotId);

        if (timeslot.IsAvailable)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TeacherId = teacherId,
                TimeslotId = timeslotId,
                Status = status, 
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Booking.AddAsync(booking);
            await unitOfWork.CommitAsync();

            timeslot.IsAvailable = false;
            await unitOfWork.Timeslot.UpdateAsync(timeslot);
            await unitOfWork.CommitAsync();

            if (user.TelegramChatId != null)
            {
                await telegramService.SendMessageAsync(user.TelegramChatId, $"Your booking is {status}. Await further updates.");
            }

            if (teacher.TelegramChatId != null)
            {
                await telegramService.SendMessageAsync(teacher.TelegramChatId, $"You have a new booking from {user.FirstName} {user.LastName}. Status: {status}.");
            }

            return booking.Id; 
        }
        else
        {
            throw new InvalidOperationException("The timeslot is no longer available.");
        }
    }
    
    public async Task<ErrorOr<Unit>> ConfirmBookingAsync(Guid bookingId, string confirmationMessage, string contactLink)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(bookingId);
        if (booking == null)
        {
            return Error.Failure("Booking not found.");
        }

        booking.Status = "Confirmed";
        booking.Message += $"\nTeacher's confirmation: {confirmationMessage}\nContact Link: {contactLink}";

        var user = await unitOfWork.Student.GetByIdAsync(booking.UserId);

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        timeslot.IsAvailable = false;
        await unitOfWork.Timeslot.UpdateAsync(timeslot);
        await unitOfWork.CommitAsync();

        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        await mediator.Publish(new BookingConfirmedEvent(bookingId, confirmationMessage, contactLink));

        return Unit.Value;
    }
}