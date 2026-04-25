using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Constants;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BuyTime_Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class CleanupJob(
    BuyTimeDbContext dbContext,
    ITelegramService telegramService,
    ILogger<CleanupJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;

        // якщо букінг був ні відхилений, ні підтверджений
        var expiredBookings = await dbContext.Bookings
            .Include(b => b.TimeSlot)
                .ThenInclude(ts => ts.Expert)
            .Include(b => b.Student)
            .Where(b => b.Status == Status.Pending && now >= b.TimeSlot.StartTime)
            .ToListAsync();

        foreach (var booking in expiredBookings)
        {
            logger.LogInformation($"Quartz: Booking {booking.Id} is expired.");
            booking.Status = Status.Expired;
            // Слот НЕ звільняємо і НЕ видаляємо (IsAvailable залишається false), бо він "заблокований" невдалою угодою
            // це потрібно, щоб у студента була історія букінгів

            _ = telegramService.NotifyBookingExpiredAsync(booking.StudentId, booking.TimeSlot.Expert.FirstName, 
                booking.TimeSlot.Expert.LastName,booking.TimeSlot.StartTime);
        }

        // ВИДАЛЕННЯ НІКОЛИ НЕ ЗАБРОНЬОВАНИХ СЛОТІВ
        // Видаляються слоти, які: закінчилися, вільні ТА не мають жодного запису в таблиці Bookings
        var ghostSlots = await dbContext.Timeslots
            .Where(ts => ts.EndTime < now && ts.IsAvailable)
            .Where(ts => !dbContext.Bookings.Any(b => b.TimeslotId == ts.Id))
            .ToListAsync();

        if (ghostSlots.Any())
        {
            logger.LogInformation($"Quartz: Deleting {ghostSlots.Count} unused ghost slots.");
            dbContext.Timeslots.RemoveRange(ghostSlots);
        }

        await dbContext.SaveChangesAsync();
    }
}