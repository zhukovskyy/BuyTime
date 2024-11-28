using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using MediatR;

namespace BuyTime_Infrastructure.Repositories;

public class TimeslotRepository(BuyTimeDbContext context)
    : Repository<Timeslot>(context), ITimeSlotRepository
{
    public async Task<ErrorOr<Unit>> UpdateAsync(Timeslot timeslot)
    {
        try
        {
            var existingTimeslot = await context.Timeslots.FindAsync(timeslot.Id);
            if(existingTimeslot == null)
                return Error.Failure("Time slot not found");
            
            existingTimeslot.StartTime = timeslot.StartTime;
            existingTimeslot.EndTime = timeslot.EndTime;
            existingTimeslot.IsAvailable = timeslot.IsAvailable;
            
            context.Timeslots.Update(existingTimeslot);
            await context.SaveChangesAsync();

            return MediatR.Unit.Value;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }
}