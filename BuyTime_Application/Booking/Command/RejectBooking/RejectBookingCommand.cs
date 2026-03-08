using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.RejectBooking;

public record RejectBookingCommand(
    Guid BookingId,
    Guid ExpertId
) : IRequest<ErrorOr<Unit>>;