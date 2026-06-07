using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Domain.Constants;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.RejectBooking;

public class RejectBookingCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService)
    : IRequestHandler<RejectBookingCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(RejectBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Booking.GetByIdAsync(request.BookingId);
        if (booking == null)
            return Error.NotFound("Booking.NotFound", "Бронювання не знайдено.");

        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(booking.TimeslotId);
        if (timeslot == null)
            return Error.NotFound("Timeslot.NotFound", "Таймслот не знайдено.");

        if (timeslot.ExpertId != request.ExpertId)
            return Error.Validation("AccessDenied", "Ви не можете відхилити чуже бронювання.");

        if (booking.Status != Status.Pending)
            return Error.Conflict("InvalidStatus", "Можна відхилити лише бронювання, яке очікує на підтвердження.");

        booking.Status = Status.Rejected;
        timeslot.IsAvailable = timeslot.StartTime > DateTime.UtcNow;

        await unitOfWork.Booking.UpdateAsync(booking);
        await unitOfWork.Timeslot.UpdateAsync(timeslot);
        await unitOfWork.CommitAsync();

        var expert = await unitOfWork.User.GetByIdAsync(request.ExpertId);
        _ = notificationService.NotifyBookingRejectedAsync(booking.StudentId, expert.FirstName, expert.LastName, timeslot.StartTime);

        return Unit.Value;
    }
}