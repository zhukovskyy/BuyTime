using MediatR;
using ErrorOr;
using System.Text.Json.Serialization;


namespace BuyTime_Application.Booking.Command.CreateBooking;

public record CreateBookingCommand(
    Guid UserId,
    Guid TimeslotId,
    string Status,
    string Message) : IRequest<ErrorOr<CreateBookingResult>>;