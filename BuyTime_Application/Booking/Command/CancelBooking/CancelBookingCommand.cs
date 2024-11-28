using ErrorOr;
using MediatR;

namespace BuyTime_Application.Booking.Command.CancelBooking;

public record CancelBookingCommand(
    Guid BookingId,
    string CancellationMessage) : IRequest<ErrorOr<Unit>>;