using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Timeslot.Command.CreateTimeslot;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Timeslot.CreateTimeslot;

public class CreateTimeslotCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTimeslotCommand, ErrorOr<CreateTimeslotResult>>
{
    public async Task<ErrorOr<CreateTimeslotResult>> Handle(CreateTimeslotCommand request,
        CancellationToken cancellationToken)
    {
        if (request.StartTime >= request.EndTime)
        {
            return Error.Failure("Invalid timeslot. Start time must be before end time.");
        }

        var timeslot = new BuyTime_Domain.Entities.Timeslot
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = true
        };

        await unitOfWork.Timeslot.AddAsync(timeslot);
        await unitOfWork.CommitAsync();

        return new CreateTimeslotResult
        {
            TimeslotId = timeslot.Id,
            UserId = timeslot.UserId,
            StartTime = timeslot.StartTime,
            EndTime = timeslot.EndTime,
            IsAvailable = timeslot.IsAvailable
        };
    }
}