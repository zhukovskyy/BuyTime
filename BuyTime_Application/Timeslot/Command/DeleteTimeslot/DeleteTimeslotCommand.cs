using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Command.DeleteTimeslot;

public record DeleteTimeslotCommand(Guid TimeslotId, Guid ExpertId) : IRequest<ErrorOr<Unit>>;