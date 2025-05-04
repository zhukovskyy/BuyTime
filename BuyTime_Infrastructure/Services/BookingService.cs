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
    public async Task<Guid> CreateBookingAsync(Guid userId,
        Guid timeslotId, string message, string status)
    {
        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(timeslotId);
        if (timeslot.IsAvailable)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TimeslotId = timeslotId,
                Message = message,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                Answer = null,
                UrlOfMeeting = null,
            };

            await unitOfWork.Booking.AddAsync(booking);
            await unitOfWork.CommitAsync();

            timeslot.IsAvailable = false;
            await unitOfWork.Timeslot.UpdateAsync(timeslot);
            await unitOfWork.CommitAsync();

            // if (user.TelegramChatId != null)
            // {
            //     await telegramService.SendMessageAsync(user.TelegramChatId, $"Your booking is {status}. Await further updates.");
            // }
            //
            // if (teacher.TelegramChatId != null)
            // {
            //     await telegramService.SendMessageAsync(teacher.TelegramChatId, $"You have a new booking from {user.FirstName} {user.LastName}. Status: {status}.");
            // }

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
        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        if (timeslot == null)
        {
            return Error.Failure("Timeslot not found.");
        }

        booking.Status = "confirmed";
        booking.Answer = confirmationMessage; 
        booking.UrlOfMeeting = contactLink;

        var user = await unitOfWork.User.GetByIdAsync(booking.UserId);

        //var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        timeslot.IsAvailable = false;
        await unitOfWork.Timeslot.UpdateAsync(timeslot);
        await unitOfWork.CommitAsync();

        await unitOfWork.Booking.UpdateAsync(booking); // answer не записується в бд, url тоже
        await unitOfWork.CommitAsync();

        await mediator.Publish(new BookingConfirmedEvent(bookingId, confirmationMessage, contactLink));

        return Unit.Value;
    }
    
    public async Task<ErrorOr<Unit>> CancelBookingAsync(Guid bookingId, string cancellationMessage)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(bookingId);
        if (booking == null)
        {
            return Error.Failure("Booking not found.");
        }

        booking.Status = "cancelled";
        booking.Message += $"\nCancellation Message: {cancellationMessage}";

        var user = await unitOfWork.User.GetByIdAsync(booking.UserId);
        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);

        timeslot.IsAvailable = true;
        await unitOfWork.Timeslot.UpdateAsync(timeslot);
        await unitOfWork.CommitAsync();

        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        if (user.TelegramChatId != null)
        {
            await telegramService.SendMessageAsync(user.TelegramChatId, $"Your booking has been cancelled. Reason: {cancellationMessage}");
        }

        return Unit.Value;
    }

}