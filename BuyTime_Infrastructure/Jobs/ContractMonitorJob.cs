using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using BuyTime_Domain.Enums;
using BuyTime_Infrastructure.Common.Persistence;
using BuyTime_Infrastructure.Common.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using TonSdk.Client;
using TonSdk.Core;

namespace BuyTime_Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class ContractMonitorJob(
    BuyTimeDbContext dbContext,
    INotificationService notificationService,
    IOptions<TonSettings> tonSettingsOptions,
    ILogger<ContractMonitorJob> logger) : IJob
{
    private TransactionRecord CreateRefundRecord(Booking booking) => new TransactionRecord
    {
        Id = Guid.NewGuid(),
        UserId = booking.StudentId,
        Type = TransactionType.Refund,
        Amount = booking.TimeSlot.Price,
        Currency = booking.TimeSlot.Currency,
        CounterpartyName = $"{booking.TimeSlot.Expert.FirstName} {booking.TimeSlot.Expert.LastName}",
        ExecutedAt = DateTime.UtcNow,
        ContractAddress = booking.ContractAddress,
        BookingId = booking.Id
    };
    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var tonSettings = tonSettingsOptions.Value;

        var tonClient = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, new HttpParameters
        {
            Endpoint = tonSettings.IsTestnet ? "https://testnet.toncenter.com/api/v2/jsonRPC" : "https://toncenter.com/api/v2/jsonRPC",
            ApiKey = tonSettings.ApiKey
        });

        // =========================================================
        // 1. МОНІТОРИНГ ЗАВЕРШЕННЯ, СКАСУВАННЯ ТА РЕФАНДУ
        // =========================================================
        var pendingBookings = await dbContext.Bookings
            .AsTracking()
            .Include(b => b.Cancellation)
            .Include(b => b.RefundRequest)
            .Include(b => b.TimeSlot)
                .ThenInclude(ts => ts.Expert)
            .Include(b => b.Student)
            .Where(b => b.Status == Status.CancelPending ||
                        b.Status == Status.RefundPending ||
                        b.Status == Status.FailedMeetingRefundPending ||
                        b.Status == Status.CompletionPending)
            .ToListAsync(ct);

        var bookingsToNotifyCancel = new List<Booking>();
        var bookingsToNotifyRefund = new List<Booking>();
        var bookingsToNotifyFailedMeeting = new List<Booking>();
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

            bool isContractEmpty = addressInfo.Value.State.ToString().Contains("uninit", StringComparison.OrdinalIgnoreCase) || currentBalance < 0.01m;

            if (isContractEmpty)
            {
                logger.LogInformation($"Quartz: Smart contract {booking.ContractAddress} is empty. Confirming logic for Booking: {booking.Id}");

                if (booking.Status == Status.CompletionPending)
                {
                    booking.Status = Status.Completed;
                    bookingsToNotifyComplete.Add(booking);

                    dbContext.TransactionRecords.Add(new TransactionRecord
                    {
                        Id = Guid.NewGuid(),
                        UserId = booking.TimeSlot.ExpertId,
                        Type = TransactionType.Received,
                        Amount = booking.TimeSlot.Price,
                        Currency = booking.TimeSlot.Currency,
                        CounterpartyName = $"{booking.Student.FirstName} {booking.Student.LastName}",
                        ExecutedAt = DateTime.UtcNow,
                        ContractAddress = booking.ContractAddress,
                        BookingId = booking.Id
                    });
                }
                else if (booking.Status == Status.RefundPending)
                {
                    booking.Status = Status.Refunded;

                    if (booking.RefundRequest != null)
                    {
                        dbContext.RefundRequests.Remove(booking.RefundRequest);
                    }

                    bookingsToNotifyRefund.Add(booking);
                    dbContext.TransactionRecords.Add(CreateRefundRecord(booking));
                }
                else if (booking.Status == Status.FailedMeetingRefundPending)
                {
                    booking.Status = Status.Refunded;

                    if (booking.RefundRequest != null)
                    {
                        dbContext.RefundRequests.Remove(booking.RefundRequest);
                    }

                    bookingsToNotifyFailedMeeting.Add(booking);
                    dbContext.TransactionRecords.Add(CreateRefundRecord(booking));
                }
                else
                {
                    booking.Status = Status.Cancelled;
                    bookingsToNotifyCancel.Add(booking);
                    if (booking.TimeSlot.StartTime > DateTime.UtcNow)
                    {
                        booking.TimeSlot.IsAvailable = true;
                    }

                    if (booking.Cancellation != null)
                    {
                        decimal refund = booking.Cancellation.RefundAmountToStudent;
                        decimal compensation = booking.Cancellation.CompensationAmountToExpert;

                        if (refund > 0)
                        {
                            dbContext.TransactionRecords.Add(new TransactionRecord
                            {
                                Id = Guid.NewGuid(),
                                UserId = booking.StudentId,
                                Type = TransactionType.Refund,
                                Amount = refund,
                                Currency = booking.TimeSlot.Currency,
                                CounterpartyName = $"{booking.TimeSlot.Expert.FirstName} {booking.TimeSlot.Expert.LastName}",
                                ExecutedAt = DateTime.UtcNow,
                                ContractAddress = booking.ContractAddress,
                                BookingId = booking.Id
                            });
                        }

                        if (compensation > 0)
                        {
                            dbContext.TransactionRecords.Add(new TransactionRecord
                            {
                                Id = Guid.NewGuid(),
                                UserId = booking.TimeSlot.ExpertId,
                                Type = TransactionType.Received,
                                Amount = compensation,
                                Currency = booking.TimeSlot.Currency,
                                CounterpartyName = $"{booking.Student.FirstName} {booking.Student.LastName}",
                                ExecutedAt = DateTime.UtcNow,
                                ContractAddress = booking.ContractAddress,
                                BookingId = booking.Id
                            });
                        }
                    }
                }
            }
            else
            {
                if (booking.Status == Status.CancelPending && booking.Cancellation != null)
                {
                    var timeSinceCancelRequest = DateTime.UtcNow - booking.Cancellation.CancelledAt;
                    if (timeSinceCancelRequest.TotalMinutes > 5)
                    {
                        logger.LogWarning($"Quartz: Timeout for Booking {booking.Id}. User didn't sign cancel tx. Reverting to Confirmed.");
                        booking.Status = Status.Confirmed;
                        dbContext.BookingCancellations.Remove(booking.Cancellation);
                    }
                }
                else if (booking.Status == Status.RefundPending && booking.RefundRequest != null)
                {
                    var timeSinceRefundRequest = DateTime.UtcNow - booking.RefundRequest.RequestedAt;
                    if (timeSinceRefundRequest.TotalMinutes > 5)
                    {
                        logger.LogWarning($"Quartz: Timeout for Booking {booking.Id}. User didn't sign refund tx. Reverting to {booking.RefundRequest.PreviousStatus}.");

                        booking.Status = booking.RefundRequest.PreviousStatus;

                        dbContext.RefundRequests.Remove(booking.RefundRequest);
                    }
                }
            }
        }

        if (pendingBookings.Any())
        {
            await dbContext.SaveChangesAsync(ct);

            foreach (var b in bookingsToNotifyComplete)
            {
                _ = notificationService.NotifyMeetingAutoResolvedAsync(
                    b.TimeSlot.ExpertId, b.Student.FirstName, b.Student.LastName,
                    b.TimeSlot.StartTime, b.TimeSlot.Price, b.TimeSlot.Currency, true);
            }

            // (Expired / Rejected)
            foreach (var b in bookingsToNotifyRefund)
            {
                _ = notificationService.NotifyRefundReceivedAsync(b.StudentId, b.TimeSlot.Price, b.TimeSlot.Currency);
            }

            // (FailedMeetingRefundPending)
            foreach (var b in bookingsToNotifyFailedMeeting)
            {
                _ = notificationService.NotifyStudentAutoRefundAsync(
                    b.StudentId, b.TimeSlot.Expert.FirstName, b.TimeSlot.Expert.LastName,
                    b.TimeSlot.StartTime, b.TimeSlot.Price, b.TimeSlot.Currency);

                _ = notificationService.NotifyMeetingAutoResolvedAsync(
                    b.TimeSlot.ExpertId, b.Student.FirstName, b.Student.LastName,
                    b.TimeSlot.StartTime, b.TimeSlot.Price, b.TimeSlot.Currency, false);
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

                    decimal refundAmount = b.Cancellation.RefundAmountToStudent;
                    decimal compensationAmount = b.Cancellation.CompensationAmountToExpert;
                    string currency = b.TimeSlot.Currency;

                    var timeBeforeMeeting = b.TimeSlot.StartTime - b.Cancellation.CancelledAt;
                    double hoursBefore = timeBeforeMeeting.TotalHours > 0 ? timeBeforeMeeting.TotalHours : 0;

                    _ = notificationService.NotifyBookingCancelledAsync(
                        targetUserId,
                        roleStr,
                        cancelledByName,
                        b.TimeSlot.StartTime,
                        b.Cancellation.Reason,
                        refundAmount,
                        compensationAmount,
                        currency,
                        hoursBefore);
                }
            }
        }

        // =========================================================
        // 2. МОНІТОРИНГ СТВОРЕННЯ БУКІНГУ (ОЧІКУВАННЯ ОПЛАТИ)
        // =========================================================
        var unpaidBookings = await dbContext.Bookings
            .AsTracking()
            .Include(b => b.TimeSlot)
                .ThenInclude(ts => ts.Expert)
            .Include(b => b.Student)
            .Where(b => b.Status == Status.PaymentPending)
            .ToListAsync(ct);

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
                    logger.LogInformation($"Quartz: Smart contract {booking.ContractAddress} funded! Moving Booking {booking.Id} to Pending.");
                    booking.Status = Status.Pending;

                    dbContext.TransactionRecords.Add(new TransactionRecord
                    {
                        Id = Guid.NewGuid(),
                        UserId = booking.StudentId,
                        Type = TransactionType.Sent,
                        Amount = booking.TimeSlot.Price,
                        Currency = booking.TimeSlot.Currency,
                        CounterpartyName = $"{booking.TimeSlot.Expert.FirstName} {booking.TimeSlot.Expert.LastName}",
                        ExecutedAt = DateTime.UtcNow,
                        ContractAddress = booking.ContractAddress,
                        BookingId = booking.Id
                    });

                    await dbContext.SaveChangesAsync(ct);

                    _ = notificationService.NotifyBookingCreatedAsync(
                        booking.TimeSlot.ExpertId, booking.Student.FirstName, booking.Student.LastName, booking.TimeSlot.StartTime);
                    continue;
                }
            }

            var timeSinceCreation = DateTime.UtcNow - booking.CreatedAt;
            if (timeSinceCreation.TotalMinutes > 5)
            {
                logger.LogWarning($"Quartz: Timeout for Booking {booking.Id}. User didn't pay. Deleting booking and freeing timeslot.");
                booking.TimeSlot.IsAvailable = true;
                dbContext.Bookings.Remove(booking);
            }
        }

        if (unpaidBookings.Any())
        {
            await dbContext.SaveChangesAsync(ct);
        }
    }
}