using MediatR;
using ErrorOr;

namespace BuyTime_Application.Booking.Command.CreateBooking;

public record CreateBookingCommand(
    Guid UserId,
    Guid TimeslotId,
    string Status,
    string Message,
    string UrlOfMeeting) : IRequest<ErrorOr<CreateBookingResult>>;