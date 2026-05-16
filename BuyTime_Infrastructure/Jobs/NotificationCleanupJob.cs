using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BuyTime_Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class NotificationCleanupJob(
    BuyTimeDbContext dbContext,
    ILogger<NotificationCleanupJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-24);

        var expiredNotifications = await dbContext.Notifications
            .Where(n => n.IsRead && n.ReadAt <= cutoffTime)
            .ToListAsync();

        if (expiredNotifications.Any())
        {
            logger.LogInformation($"Quartz [Cleanup]: Deleting {expiredNotifications.Count} read notifications older than 24h.");
            dbContext.Notifications.RemoveRange(expiredNotifications);
            await dbContext.SaveChangesAsync();
        }
    }
}