using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Entities;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuyTime_Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IServiceScopeFactory scopeFactory, ILogger<NotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    private string FormatLocalTime(DateTime utcTime, UserSettings? settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.Timezone) || settings.Timezone == "UTC")
        {
            return $"{utcTime:dd.MM.yyyy HH:mm} (UTC)";
        }

        try
        {
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(settings.Timezone);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzInfo);

            var parts = settings.Timezone.Split('/');
            var cityName = parts.Last().Replace("_", " ");

            return $"{localTime:dd.MM.yyyy HH:mm} ({cityName})";
        }
        catch
        {
            return $"{utcTime:dd.MM.yyyy HH:mm} (UTC)";
        }
    }

    private async Task DispatchNotificationAsync(
        Guid userId,
        string title,
        Func<UserSettings?, string> messageGenerator,
        string type,
        Func<UserSettings, bool> settingPredicate)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BuyTimeDbContext>();
            var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();

            var user = await dbContext.Users
                .Include(u => u.Settings)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return;

            if (user.Settings != null && !settingPredicate(user.Settings)) return;

            string generatedMessage = messageGenerator(user.Settings);

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = generatedMessage,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            if (user.Settings != null && user.Settings.NotifyInTelegram && !string.IsNullOrEmpty(user.TelegramChatId))
            {
                await telegramService.SendMessageAsync(user.TelegramChatId, $"<b>{title}</b>\n{generatedMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in DispatchNotificationAsync: {ex.Message}");
        }
    }

    public Task NotifyBookingCreatedAsync(Guid expertId, string studentFirstName, string studentLastName, DateTime startTime)
    {
        var title = "📅 Нове бронювання!";
        return DispatchNotificationAsync(expertId, title,
            settings => $"Студент {studentFirstName} {studentLastName} забронював зустріч на {FormatLocalTime(startTime, settings)}.",
            "BookingCreated", s => s.NotifyOnBooking);
    }

    public Task NotifyBookingCancelledAsync(
        Guid targetUserId,
        string cancelledByRole,
        string cancelledByName,
        DateTime startTime,
        string reason,
        decimal? refundAmount = null,
        decimal? compensationAmount = null,
        string? currency = null,
        double? hoursBefore = null)
    {
        bool targetIsExpert = cancelledByRole.ToLower() == "student";
        var roleName = targetIsExpert ? "Студент" : "Експерт";
        var title = "⚠️ Бронювання скасовано";

        string timeInfo = "";
        if (hoursBefore.HasValue)
        {
            int hours = (int)hoursBefore.Value;
            timeInfo = hours > 0
                ? $" (за {hours} год. до початку)"
                : " (менш ніж за годину до початку)";
        }

        return DispatchNotificationAsync(targetUserId, title, settings =>
        {
            var msg = $"{roleName} {cancelledByName} скасував зустріч на {FormatLocalTime(startTime, settings)}{timeInfo}.\nПричина: {reason}";

            if (targetIsExpert && compensationAmount.HasValue && compensationAmount.Value > 0 && !string.IsNullOrEmpty(currency))
            {
                msg += $"\n\n💸 Компенсація: {compensationAmount.Value:0.####} {currency} успішно зараховано на ваш гаманець.";
            }
            else if (!targetIsExpert && refundAmount.HasValue && refundAmount.Value > 0 && !string.IsNullOrEmpty(currency))
            {
                msg += $"\n\n💸 Повернення коштів: {refundAmount.Value:0.####} {currency} успішно повернуто на ваш гаманець.";
            }
            return msg;
        }, "BookingCancelled", s => s.NotifyOnBooking);
    }

    public Task NotifyBookingRejectedAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime)
    {
        var title = "❌ Зустріч відхилено";
        return DispatchNotificationAsync(studentId, title,
            settings => $"Експерт {expertFirstName} {expertLastName} не зміг підтвердити зустріч на {FormatLocalTime(startTime, settings)}. Ви можете повернути кошти в деталях цієї зустрічі.",
            "BookingRejected", s => s.NotifyOnBooking);
    }

    public Task NotifyBookingConfirmedAsync(Guid studentId, string studentFirstName, string studentLastName, Guid expertId, string expertFirstName, string expertLastName, DateTime startTime, string? messageToStudent, string? meetingLink)
    {
        var linkText = string.IsNullOrEmpty(meetingLink) ? "" : $"Посилання: {meetingLink}";
        var expertMessageText = string.IsNullOrEmpty(messageToStudent) ? "" : messageToStudent;

        var studentTitle = "✅ Зустріч підтверджено!";
        var expertTitle = "✅ Ви підтвердили зустріч!";

        var task1 = DispatchNotificationAsync(studentId, studentTitle,
            settings => $"Експерт {expertFirstName} {expertLastName} підтвердив зустріч на {FormatLocalTime(startTime, settings)}.\nПовідомлення від експерта: {expertMessageText}\n{linkText}",
            "BookingConfirmed", s => s.NotifyOnBooking);

        var task2 = DispatchNotificationAsync(expertId, expertTitle,
            settings => $"Зустріч зі студентом {studentFirstName} {studentLastName} на {FormatLocalTime(startTime, settings)}.\n{linkText}",
            "BookingConfirmed", s => s.NotifyOnBooking);

        return Task.WhenAll(task1, task2);
    }

    public Task NotifyRefundReceivedAsync(Guid studentId, decimal amount, string currency)
    {
        var title = "💸 Повернення коштів";
        return DispatchNotificationAsync(studentId, title,
            _ => $"Сума {amount:0.####} {currency} була успішно повернута на ваш гаманець.",
            "FinanceRefund", s => s.NotifyOnFinance);
    }

    public Task NotifyBookingExpiredAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime)
    {
        var title = "⚠️ Зустріч скасовано системою";
        return DispatchNotificationAsync(studentId, title,
            settings => $"Експерт {expertFirstName} {expertLastName} не підтвердив вашу зустріч на {FormatLocalTime(startTime, settings)}. Зайдіть у деталі зустрічі, щоб повернути свої кошти.",
            "BookingExpired", s => s.NotifyOnBooking);
    }

    public Task NotifyMeetingResolvedByStudentAsync(Guid expertId, string studentFirstName, string studentLastName, DateTime startTime, decimal amount, string currency, bool isSuccessful)
    {
        var title = isSuccessful ? "✅ Зустріч успішно завершена!" : "⚠️ Зустріч скасована";
        return DispatchNotificationAsync(expertId, title, settings =>
        {
            string timeString = FormatLocalTime(startTime, settings);
            return isSuccessful
                ? $"Студент {studentFirstName} {studentLastName} підтвердив проведення зустрічі ({timeString}).\n💸 {amount:0.####} {currency} відправлено на ваш гаманець."
                : $"Студент {studentFirstName} {studentLastName} вказав, що зустріч ({timeString}) не відбулася.\n💸 {amount:0.####} {currency} повернуто студенту.";
        }, "MeetingResolved", s => s.NotifyOnBooking);
    }

    public Task NotifyMeetingAutoResolvedAsync(Guid expertId, string studentFirstName, string studentLastName, DateTime startTime, decimal amount, string currency, bool isSuccessful)
    {
        var title = isSuccessful ? "✅ Зустріч успішно завершена!" : "⚠️ Зустріч скасована системою";
        return DispatchNotificationAsync(expertId, title, settings =>
        {
            string timeString = FormatLocalTime(startTime, settings);
            return isSuccessful
                ? $"💸 {amount:0.####} {currency} відправлено на ваш гаманець."
                : $"На основі даних Discord виявлено, що Ви були відсутні на зустрічі ({timeString}).\n💸 {amount:0.####} {currency} повернуто студенту.";
        }, "MeetingAutoResolved", s => s.NotifyOnBooking);
    }

    public Task NotifyStudentAutoRefundAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime, decimal amount, string currency)
    {
        var title = "⚠️ Зустріч не відбулася";
        return DispatchNotificationAsync(studentId, title,
            settings => $"Система зафіксувала, що експерт {expertFirstName} {expertLastName} не з'явився на заплановану зустріч ({FormatLocalTime(startTime, settings)}) у Discord.\n\n💸 {amount:0.####} {currency} успішно повернуто на ваш гаманець.",
            "StudentAutoRefund", s => s.NotifyOnFinance);
    }

    public Task NotifyNewFeedbackAsync(Guid expertId, string studentFirstName, string studentLastName, decimal rating, string? comment)
    {
        var title = "⭐ Новий відгук!";
        return DispatchNotificationAsync(expertId, title,
            _ => $"Студент {studentFirstName} {studentLastName} залишив вам відгук.\nОцінка: {rating}/5\nКоментар: {comment ?? "Без коментаря"}",
            "NewFeedback", s => s.NotifyOnNewFeedback);
    }
}