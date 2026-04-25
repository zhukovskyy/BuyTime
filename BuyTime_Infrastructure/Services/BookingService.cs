using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Events;
using BuyTime_Domain.Entities;
using BuyTime_Domain.Enums;
using MediatR;
using ErrorOr;
using BuyTime_Domain.Constants;

namespace BuyTime_Infrastructure.Services;

public class BookingService(
    IUnitOfWork unitOfWork,
    IMediator mediator) : IBookingService
{
    public async Task<Guid> CreateBookingAsync(Guid studentId,
        Guid timeslotId, string messageToExpert, string contractAddress, string studentWalletAddress)
    {
        if (string.IsNullOrEmpty(contractAddress))
        {
            throw new ArgumentException("Contract hash is required for booking.");
        }

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(timeslotId);

        if (timeslot.IsAvailable)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                TimeslotId = timeslotId,
                ContractAddress = contractAddress,
                MessageToExpert = messageToExpert,
                Status = Status.PaymentPending,
                CreatedAt = DateTime.UtcNow,
                ConfirmationMessage = null,
                MeetingLink = null,
                StudentWalletAddress = studentWalletAddress
            };

            await unitOfWork.Booking.AddAsync(booking);

            timeslot.IsAvailable = false;

            await unitOfWork.Timeslot.UpdateAsync(timeslot);
            await unitOfWork.CommitAsync();

            await mediator.Publish(new BookingCreatedEvent(booking.Id));

            return booking.Id;
        }
        else
        {
            throw new InvalidOperationException("The timeslot is already booked.");
        }
    }

    public async Task<ErrorOr<Unit>> ConfirmBookingAsync(
    Guid bookingId,
    string confirmationMessage,
    string meetingLink,
    MeetingPlatform platform,
    string? externalMeetingId)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(bookingId);
        if (booking == null) return Error.Failure("Booking not found.");

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        if (timeslot == null) return Error.Failure("Timeslot not found.");

        booking.Status = Status.Confirmed;
        booking.ConfirmationMessage = confirmationMessage;
        booking.MeetingLink = meetingLink;

        if (!string.IsNullOrEmpty(externalMeetingId))
        {
            var attendanceMarker = new MeetingAttendance
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ExternalMeetingId = externalMeetingId,
                Platform = platform,
                ExternalUserId = 0,
                SystemUserId = null,
                FirstJoinedAt = DateTime.UtcNow
            };

            booking.Attendances = new List<MeetingAttendance> { attendanceMarker };
        }

        timeslot.IsAvailable = false;

        await unitOfWork.Timeslot.UpdateAsync(timeslot);
        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        await mediator.Publish(new BookingConfirmedEvent(bookingId, confirmationMessage, meetingLink));

        return Unit.Value;
    }

    public async Task<ErrorOr<Unit>> CancelBookingAsync(Guid bookingId, string reason, Guid triggeredByUserId)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(bookingId);
        if (booking == null) return Error.Failure("Booking not found.");

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);

        if (triggeredByUserId != booking.StudentId && triggeredByUserId != timeslot.ExpertId)
        {
            return Error.Validation("AccessDenied", "Ви не є учасником цього бронювання.");
        }

        booking.Status = Status.Cancelled;

        var cancellation = new BookingCancellation
        {
            BookingId = booking.Id,
            Reason = reason,
            CancelledAt = DateTime.UtcNow,
            CancelledByUserId = triggeredByUserId 
        };

        booking.Cancellation = cancellation; 

        timeslot.IsAvailable = true;

        await unitOfWork.Timeslot.UpdateAsync(timeslot);
        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.CommitAsync();

        await mediator.Publish(new BookingCancelledEvent(bookingId, reason, triggeredByUserId));

        return Unit.Value;
    }
}