using BuyTime_Application.Timeslot.CreateTimeslot;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Command.CreateTimeslot;

public record CreateTimeslotCommand(
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime
) : IRequest<ErrorOr<CreateTimeslotResult>>;