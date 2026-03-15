using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Repositories;

public class TimeslotRepository(BuyTimeDbContext context)
    : Repository<Timeslot>(context), ITimeSlotRepository
{
    public async Task<ErrorOr<Unit>> UpdateAsync(Timeslot timeslot)
    {
        try
        {
            var existingTimeslot = await context.Timeslots.FindAsync(timeslot.Id);
            if (existingTimeslot == null)
                return Error.Failure("Time slot not found");

            existingTimeslot.StartTime = timeslot.StartTime;
            existingTimeslot.EndTime = timeslot.EndTime;
            existingTimeslot.IsAvailable = timeslot.IsAvailable;

            existingTimeslot.Price = timeslot.Price;
            existingTimeslot.Currency = timeslot.Currency;
            existingTimeslot.ExpertWalletAddress = timeslot.ExpertWalletAddress;

            context.Timeslots.Update(existingTimeslot);
            await context.SaveChangesAsync();

            return MediatR.Unit.Value;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<Timeslot>>> GetByExpertIdAsync(Guid expertId)
    {
        try
        {
            var timeslots = await dbSet
                .Where(ts => ts.ExpertId == expertId && !ts.Bookings.Any(b=>b.Status == Status.Completed))
                .Include(ts => ts.Bookings)
                    .ThenInclude(b => b.Student)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();

            return timeslots;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message);
        }
    }

    public async Task<bool> HasOverlappingAsync(Guid expertId, DateTime startTime, DateTime endTime, Guid? excludeTimeslotId = null)
    {
        var query = dbSet.Where(ts => ts.ExpertId == expertId);

        if (excludeTimeslotId.HasValue)
        {
            query = query.Where(ts => ts.Id != excludeTimeslotId.Value);
        }

        return await query.AnyAsync(ts => ts.StartTime < endTime && startTime < ts.EndTime);
    }
}