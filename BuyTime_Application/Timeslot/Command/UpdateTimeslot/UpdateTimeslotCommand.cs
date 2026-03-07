using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Command.UpdateTimeslot;

public record UpdateTimeslotCommand(
    Guid TimeslotId,
    Guid ExpertId,
    DateTime StartTime,
    DateTime EndTime,
    decimal Price,
    string Currency
) : IRequest<ErrorOr<Unit>>;