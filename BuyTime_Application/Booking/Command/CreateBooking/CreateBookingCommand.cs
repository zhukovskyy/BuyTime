using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.CreateBooking;

public record CreateBookingCommand(
    // Guid TeacherId,
    Guid UserId,
    Guid TimeslotId,
    string Status,
    string Message) : IRequest<ErrorOr<CreateBookingResult>>;