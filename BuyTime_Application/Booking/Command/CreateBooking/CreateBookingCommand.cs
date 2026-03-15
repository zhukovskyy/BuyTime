using BuyTime_Application.Booking.Command.CreateBooking;
using ErrorOr;
using MediatR;

public record CreateBookingCommand(
    Guid StudentId,
    Guid TimeslotId,
    string MessageToExpert
) : IRequest<ErrorOr<CreateBookingResult>>;