using BuyTime_Application.Booking.Command.CreateBooking;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.CreateTimeslot;

public record CreateTimeslotCommand(
    Guid TeacherId,
    DateTime StartTime,
    DateTime EndTime
) : IRequest<ErrorOr<CreateTimeslotResult>>;