using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Enums;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BuyTime_Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class MeetingCleanupJob(
    BuyTimeDbContext dbContext,
    IDiscordService discordService,
    ILogger<MeetingCleanupJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bufferTime = DateTime.UtcNow.AddMinutes(-15);

        var expiredMarkers = await dbContext.MeetingAttendances
            .Include(ma => ma.Booking)
                .ThenInclude(b => b.TimeSlot)
            .Where(ma => ma.ExternalUserId == 0) // Тільки маркери створення
            .Where(ma => ma.Booking.TimeSlot.EndTime <= bufferTime)
            .ToListAsync();

        foreach (var marker in expiredMarkers)
        {
            try
            {
                if (marker.Platform == MeetingPlatform.Discord)
                {
                    ulong discordId = ulong.Parse(marker.ExternalMeetingId);
                    bool isEmpty = await discordService.IsMeetingEmptyAsync(discordId);

                    if (isEmpty)
                    {
                        logger.LogInformation($"Quartz [Cleanup]: Discord channel {discordId} is empty. Deleting...");
                        await discordService.FinishMeetingAsync(discordId);

                        var allEntries = await dbContext.MeetingAttendances
                            .Where(ma => ma.ExternalMeetingId == marker.ExternalMeetingId)
                            .ToListAsync();

                        dbContext.MeetingAttendances.RemoveRange(allEntries);
                    }
                    else
                    {
                        logger.LogInformation($"Quartz [Cleanup]: Channel {discordId} still active. Skipping.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error cleaning up meeting {marker.ExternalMeetingId}: {ex.Message}");
            }
        }

        await dbContext.SaveChangesAsync();
    }
}