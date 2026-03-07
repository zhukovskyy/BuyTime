using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.Command.DeleteTimeslot;

public class DeleteTimeslotCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTimeslotCommand, ErrorOr<Unit>>
{
    public async Task<ErrorOr<Unit>> Handle(DeleteTimeslotCommand request, CancellationToken cancellationToken)
    {
        var timeslot = await unitOfWork.Timeslot.GetByIdAsync(request.TimeslotId);

        if (timeslot == null)
        {
            return Error.NotFound("Timeslot.NotFound", "Таймслот не знайдено.");
        }

        if (timeslot.ExpertId != request.ExpertId)
        {
            return Error.Validation("AccessDenied", "Ви не можете видалити чужий таймслот.");
        }

        if (!timeslot.IsAvailable)
        {
            return Error.Conflict("TimeslotBooked", "Неможливо видалити таймслот, який вже заброньовано. Спочатку скасуйте бронювання.");
        }

        await unitOfWork.Timeslot.DeleteAsync(timeslot);
        await unitOfWork.CommitAsync();

        return Unit.Value;
    }
}