using BuyTime_Domain.Constants;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

                var tonClient = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, new HttpParameters
                {
                    Endpoint = "https://testnet.toncenter.com/api/v2/jsonRPC"
                });

                // =========================================================
                // МОНІТОРИНГ СКАСУВАННЯ ТА РЕФАНДУ (ОЧІКУВАННЯ ОПЛАТИ)
                // =========================================================

                var pendingBookings = await dbContext.Bookings
                    .Include(b => b.Cancellation)
                    .Where(b => b.Status == Status.CancelPending || b.Status == Status.RefundPending)
                    .ToListAsync(stoppingToken);

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
                        if (booking.Status == Status.RefundPending)
                        {
                            booking.Status = Status.Refunded;
                        }
                        else
                        {
                            booking.Status = Status.Cancelled;
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
                }

                // =========================================================
                // МОНІТОРИНГ СТВОРЕННЯ БУКІНГУ (ОЧІКУВАННЯ ОПЛАТИ)
                // =========================================================
                var unpaidBookings = await dbContext.Bookings
                    .Include(b => b.TimeSlot)
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

                            // TODO: сповіщення
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