using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.ConfirmBooking;

public record ConfirmBookingCommand(
    Guid BookingId,
    string ConfirmationMessage,
    string ContactLink) : IRequest<ErrorOr<Unit>>;