using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using BuyTime_Infrastructure.Common.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TonSdk.Client;
using TonSdk.Core;

namespace BuyTime_Infrastructure.Services;

public class TonContractMonitorService(
    IServiceProvider serviceProvider,
    ILogger<TonContractMonitorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TON Contract Monitor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BuyTimeDbContext>();

                var tonSettings = scope.ServiceProvider.GetRequiredService<IOptions<TonSettings>>().Value;
                var tonClient = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, new HttpParameters
                {
                    Endpoint = "https://testnet.toncenter.com/api/v2/jsonRPC",
                    ApiKey = tonSettings.ApiKey
                });

                // =========================================================
                // МОНІТОРИНГ ЗАВЕРШЕННЯ, СКАСУВАННЯ ТА РЕФАНДУ (ОЧІКУВАННЯ ОПЛАТИ)
                // =========================================================

                var pendingBookings = await dbContext.Bookings
                    .Include(b => b.Cancellation)
                    .Include(b => b.TimeSlot)
                        .ThenInclude(ts => ts.Expert)
                    .Include(b => b.Student)
                    .Where(b => b.Status == Status.CancelPending || b.Status == Status.RefundPending || b.Status == Status.CompletionPending).ToListAsync(stoppingToken);

                var bookingsToNotifyCancel = new List<Booking>();
                var bookingsToNotifyRefund = new List<Booking>();
                var bookingsToNotifyComplete = new List<Booking>();

                foreach (var booking in pendingBookings)
                {
                    var addressInfo = await tonClient.GetAddressInformation(new Address(booking.ContractAddress));

                    if (addressInfo == null) continue;

                    decimal currentBalance = 0;
                    string balanceStr = addressInfo.Value.Balance.ToString().Replace(',', '.');
                    if (decimal.TryParse(balanceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var bal))
                    {
                        currentBalance = bal;
                    }

                    bool isContractEmpty = addressInfo.Value.State.ToString().Contains("uninit", StringComparison.OrdinalIgnoreCase) ||
                                           currentBalance < 0.01m;
                    if (isContractEmpty)
                    {
                        logger.LogInformation($"Smart contract {booking.ContractAddress} is empty. Confirming cancellation for Booking: {booking.Id}");

                        if (booking.Status == Status.CompletionPending)
                        {
                            booking.Status = Status.Completed;
                            bookingsToNotifyComplete.Add(booking);
                        }
                        else if (booking.Status == Status.RefundPending)
                        {
                            booking.Status = Status.Refunded;
                            bookingsToNotifyRefund.Add(booking);
                            booking.TimeSlot.IsAvailable = true;
                        }
                        else
                        {
                            booking.Status = Status.Cancelled;
                            bookingsToNotifyCancel.Add(booking);
                            booking.TimeSlot.IsAvailable = true;
                        }

                        dbContext.Bookings.Update(booking);
                    }
                    else
                    {
                        if (booking.Cancellation != null)
                        {
                            var timeSinceCancelRequest = DateTime.UtcNow - booking.Cancellation.CancelledAt;
                            // якщо підтвердження немає більше 5 хв або користувач взагалі закрив ТОН гаманець
                            // TODO: може це якось гарніше можна буде зробити
                            if (timeSinceCancelRequest.TotalMinutes > 5)
                            {
                                logger.LogWarning($"Timeout for Booking {booking.Id}. User didn't sign cancel transaction. Reverting to Confirmed.");

                                booking.Status = Status.Confirmed;
                                dbContext.BookingCancellations.Remove(booking.Cancellation);
                                dbContext.Bookings.Update(booking);
                            }
                        }
                    }
                }

                if (pendingBookings.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);

                    var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();

                    foreach (var b in bookingsToNotifyComplete)
                    {
                        _ = telegramService.NotifyMeetingResolvedByStudentAsync(
                            b.TimeSlot.ExpertId,
                            b.Student.FirstName,
                            b.Student.LastName,
                            b.TimeSlot.StartTime,
                            b.TimeSlot.Price,
                            b.TimeSlot.Currency,
                            true);
                    }

                    foreach (var b in bookingsToNotifyRefund)
                    {
                        _ = telegramService.NotifyRefundReceivedAsync(b.StudentId, b.TimeSlot.Price, b.TimeSlot.Currency);

                        // це повідомлення прийде експерту коли студент вибере що зустріч не відбулася
                        _ = telegramService.NotifyMeetingResolvedByStudentAsync(
                            b.TimeSlot.ExpertId,
                            b.Student.FirstName,
                            b.Student.LastName,
                            b.TimeSlot.StartTime,
                            b.TimeSlot.Price,
                            b.TimeSlot.Currency,
                            false);
                    }


                    foreach (var b in bookingsToNotifyCancel)
                    {
                        if (b.Cancellation != null)
                        {
                            bool isStudent = b.Cancellation.CancelledByUserId == b.StudentId;
                            var targetUserId = isStudent ? b.TimeSlot.ExpertId : b.StudentId;
                            var roleStr = isStudent ? "student" : "expert";

                            var cancelledByName = isStudent
                                ? $"{b.Student.FirstName} {b.Student.LastName}"
                                : $"{b.TimeSlot.Expert.FirstName} {b.TimeSlot.Expert.LastName}";

                            decimal? refundToStudent = !isStudent ? b.TimeSlot.Price : null;
                            string? currency = !isStudent ? b.TimeSlot.Currency : null;

                            _ = telegramService.NotifyBookingCancelledAsync(
                                targetUserId,
                                roleStr,
                                cancelledByName,
                                b.TimeSlot.StartTime,
                                b.Cancellation.Reason,
                                refundToStudent,
                                currency);
                        }
                    }
                }

                if (pendingBookings.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                // =========================================================
                // МОНІТОРИНГ СТВОРЕННЯ БУКІНГУ (ОЧІКУВАННЯ ОПЛАТИ)
                // =========================================================
                var unpaidBookings = await dbContext.Bookings
                    .Include(b => b.TimeSlot)
                    .Include(b => b.Student)
                    .Where(b => b.Status == Status.PaymentPending)
                    .ToListAsync(stoppingToken);

                foreach (var booking in unpaidBookings)
                {
                    var addressInfo = await tonClient.GetAddressInformation(new Address(booking.ContractAddress));

                    if (addressInfo != null && addressInfo.Value.State.ToString().Contains("active", StringComparison.OrdinalIgnoreCase))
                    {
                        decimal currentBalance = 0;
                        string balanceStr = addressInfo.Value.Balance.ToString().Replace(',', '.');
                        if (decimal.TryParse(balanceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var bal))
                        {
                            currentBalance = bal;
                        }

                        if (currentBalance >= booking.TimeSlot.Price * 0.99m)
                        {
                            logger.LogInformation($"Smart contract {booking.ContractAddress} funded! Moving Booking {booking.Id} to Pending.");

                            booking.Status = Status.Pending;
                            dbContext.Bookings.Update(booking);

                            await dbContext.SaveChangesAsync(stoppingToken);

                            var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
                            _ = telegramService.NotifyBookingCreatedAsync(
                                booking.TimeSlot.ExpertId,
                                booking.Student.FirstName,
                                booking.Student.LastName,
                                booking.TimeSlot.StartTime);

                            continue; 
                        }
                    }

                    var timeSinceCreation = DateTime.UtcNow - booking.CreatedAt;

                    if (timeSinceCreation.TotalMinutes > 5)
                    {
                        logger.LogWarning($"Timeout for Booking {booking.Id}. User didn't pay. Deleting booking and freeing timeslot.");

                        booking.TimeSlot.IsAvailable = true;
                        dbContext.Timeslots.Update(booking.TimeSlot);

                        dbContext.Bookings.Remove(booking);
                    }
                }

                if (unpaidBookings.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in TON Monitor: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}