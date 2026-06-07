using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BuyTime_Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class CleanupJob(
    BuyTimeDbContext dbContext,
    INotificationService notificationService,
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
            dbContext.Bookings.Update(booking);
            // Слот НЕ звільняємо і НЕ видаляємо (IsAvailable залишається false), бо він "заблокований" невдалою угодою
            // це потрібно, щоб у студента була історія букінгів

            _ = notificationService.NotifyBookingExpiredAsync(booking.StudentId, booking.TimeSlot.Expert.FirstName, 
                booking.TimeSlot.Expert.LastName,booking.TimeSlot.StartTime);
        }

        // ВИДАЛЕННЯ НІКОЛИ НЕ ЗАБРОНЬОВАНИХ СЛОТІВ
        // Видаляються слоти, які: закінчилися, вільні ТА не мають жодного запису в таблиці Bookings
        var outdatedSlots = await dbContext.Timeslots
            .Include(ts => ts.Bookings)
            .Where(ts => ts.StartTime <= now && ts.IsAvailable)
            .ToListAsync();

        var slotsToDelete = new List<Timeslot>();
        var slotsToDisable = new List<Timeslot>();

        foreach (var slot in outdatedSlots)
        {
            if (!slot.Bookings.Any())
            {
                slotsToDelete.Add(slot);
            }
            else
            {
                // Якщо є історія (відхилені, скасовані тощо) — просто робимо недоступним для нових бронювань
                slot.IsAvailable = false;
                slotsToDisable.Add(slot);
            }
        }

        if (slotsToDelete.Any())
        {
            logger.LogInformation($"Quartz: Deleting {slotsToDelete.Count} unused ghost slots.");
            dbContext.Timeslots.RemoveRange(slotsToDelete);
        }

        if (slotsToDisable.Any())
        {
            logger.LogInformation($"Quartz: Disabling {slotsToDisable.Count} outdated slots with history.");
            dbContext.Timeslots.UpdateRange(slotsToDisable);
        }

        await dbContext.SaveChangesAsync();
    }
}