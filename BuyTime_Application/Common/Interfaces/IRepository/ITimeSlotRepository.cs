using BuyTime_Domain.Entities;
using ErrorOr;
using MediatR;

namespace BuyTime_Application.Common.Interfaces.IRepository;

public interface ITimeSlotRepository : IRepository<BuyTime_Domain.Entities.Timeslot>
{
    Task<ErrorOr<Unit>> UpdateAsync(BuyTime_Domain.Entities.Timeslot timeslot);

    Task<ErrorOr<IEnumerable<BuyTime_Domain.Entities.Timeslot>>> GetByExpertIdAsync(Guid expertId);
    Task<bool> HasOverlappingAsync(Guid expertId, DateTime startTime, DateTime endTime, Guid? excludeTimeslotId = null);
}