using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Constants;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BuyTime_Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class BookingResolutionJob(
    BuyTimeDbContext dbContext,
    ITonContractService tonContractService,
    ILogger<BookingResolutionJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow.AddMinutes(-1);

        var expiredBookings = await dbContext.Bookings
            .AsTracking()
            .Include(b => b.TimeSlot)
            .Where(b => b.Status == Status.Confirmed && b.TimeSlot.EndTime <= now)
            .ToListAsync();

        foreach (var booking in expiredBookings)
        {
            try
            {
                logger.LogInformation($"Quartz [Payment]: Resolving booking {booking.Id}");

                bool isExpertPresent = await dbContext.MeetingAttendances
                    .AnyAsync(ma => ma.BookingId == booking.Id && ma.SystemUserId == booking.TimeSlot.ExpertId);

                booking.Status = isExpertPresent ? Status.CompletionPending : Status.FailedMeetingRefundPending;

                var resolveResult = await tonContractService.ResolveBookingByArbiterAsync(
                    booking.ContractAddress,
                    isExpertPresent);

                if (!resolveResult.IsError)
                {
                    booking.Status = isExpertPresent ? Status.CompletionPending : Status.FailedMeetingRefundPending;
                    dbContext.Bookings.Update(booking);
                    logger.LogInformation($"Quartz [Payment]: Payment triggered for {booking.Id}. Expert present: {isExpertPresent}");
                }
                else
                {
                    logger.LogError($"Quartz [Payment]: Payment failed for {booking.Id}: {resolveResult.FirstError.Description}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in payment logic for booking {booking.Id}: {ex.Message}");
            }
        }

        await dbContext.SaveChangesAsync();
    }
}