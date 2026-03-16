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

                var pendingBookings = await dbContext.Bookings
                    .Include(b => b.Cancellation)
                    .Where(b => b.Status == Status.CancelPending)
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
                        booking.Status = Status.Cancelled;
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
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in TON Monitor: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}