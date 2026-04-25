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
        var bufferTime = DateTime.UtcNow.AddMinutes(-1);

        var activeRooms = await dbContext.MeetingAttendances
            .Include(ma => ma.Booking) 
                .ThenInclude(b => b.TimeSlot)
            .Where(ma => ma.ExternalUserId == 0) // маркер створення
            .Where(ma => ma.Booking.TimeSlot.EndTime <= bufferTime)
            .ToListAsync();

        foreach (var roomMarker in activeRooms)
        {
            try
            {
                if (roomMarker.Platform == MeetingPlatform.Discord)
                {
                    ulong discordId = ulong.Parse(roomMarker.ExternalMeetingId);

                    bool isEmpty = await discordService.IsMeetingEmptyAsync(discordId);

                    if (!isEmpty)
                    {
                        logger.LogInformation($"Quartz: Discord channel {discordId} is NOT empty. Skipping deletion.");
                        continue;
                    }

                    logger.LogInformation($"Quartz: Deleting Discord channel {discordId}");
                    await discordService.FinishMeetingAsync(discordId);
                }

                var relatedAttendances = await dbContext.MeetingAttendances
                    .Where(ma => ma.ExternalMeetingId == roomMarker.ExternalMeetingId)
                    .ToListAsync();

                dbContext.MeetingAttendances.RemoveRange(relatedAttendances);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to delete {roomMarker.Platform} meeting {roomMarker.ExternalMeetingId}: {ex.Message}");
            }
        }

        await dbContext.SaveChangesAsync();
    }
}