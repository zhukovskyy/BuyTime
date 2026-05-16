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

    private async Task DispatchNotificationAsync(
        Guid userId,
        string title,
        string message,
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

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync();

            if (user.Settings != null && user.Settings.NotifyInTelegram && !string.IsNullOrEmpty(user.TelegramChatId))
            {
                await telegramService.SendMessageAsync(user.TelegramChatId, $"<b>{title}</b>\n{message}");
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
        var msg = $"Студент {studentFirstName} {studentLastName} забронював зустріч на {startTime:dd.MM HH:mm} (UTC).";
        return DispatchNotificationAsync(expertId, title, msg, "BookingCreated", s => s.NotifyOnBooking);
    }

    public Task NotifyBookingCancelledAsync(Guid targetUserId, string cancelledByRole, string cancelledByName, DateTime startTime, string reason, decimal? refundAmount = null, string? currency = null)
    {
        var roleName = cancelledByRole.ToLower() == "student" ? "Студент" : "Експерт";
        var title = "⚠️ Бронювання скасовано";
        var msg = $"{roleName} {cancelledByName} скасував зустріч на {startTime:dd.MM HH:mm} (UTC).\nПричина: {reason}";

        if (refundAmount.HasValue && !string.IsNullOrEmpty(currency))
            msg += $"\n\n💸 Повернення коштів: {refundAmount.Value} {currency} успішно повернуто на ваш гаманець.";

        return DispatchNotificationAsync(targetUserId, title, msg, "BookingCancelled", s => s.NotifyOnBooking);
    }

    public Task NotifyBookingRejectedAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime)
    {
        var title = "❌ Зустріч відхилено";
        var msg = $"Експерт {expertFirstName} {expertLastName} не зміг підтвердити зустріч на {startTime:dd.MM HH:mm} (UTC). Ви можете повернути кошти в деталях цієї зустрічі.";
        return DispatchNotificationAsync(studentId, title, msg, "BookingRejected", s => s.NotifyOnBooking);
    }

    public Task NotifyBookingConfirmedAsync(Guid studentId, string studentFirstName, string studentLastName, Guid expertId, string expertFirstName, string expertLastName, DateTime startTime, string? messageToStudent, string? meetingLink)
    {
        var linkText = string.IsNullOrEmpty(meetingLink) ? "" : $"Посилання: {meetingLink}";
        var expertMessageText = string.IsNullOrEmpty(messageToStudent) ? "" : messageToStudent;

        var studentTitle = "✅ Зустріч підтверджено!";
        var studentMsg = $"Експерт {expertFirstName} {expertLastName} підтвердив зустріч на {startTime:dd.MM HH:mm} (UTC).\nПовідомлення від експерта: {expertMessageText}\n{linkText}";

        var expertTitle = "✅ Ви підтвердили зустріч!";
        var expertMsg = $"Зустріч зі студентом {studentFirstName} {studentLastName} на {startTime:dd.MM HH:mm} (UTC).\n{linkText}";

        var task1 = DispatchNotificationAsync(studentId, studentTitle, studentMsg, "BookingConfirmed", s => s.NotifyOnBooking);
        var task2 = DispatchNotificationAsync(expertId, expertTitle, expertMsg, "BookingConfirmed", s => s.NotifyOnBooking);

        return Task.WhenAll(task1, task2);
    }

    public Task NotifyRefundReceivedAsync(Guid studentId, decimal amount, string currency)
    {
        var title = "💸 Повернення коштів";
        var msg = $"Сума {amount:0.####} {currency} була успішно повернута на ваш гаманець.";
        return DispatchNotificationAsync(studentId, title, msg, "FinanceRefund", s => s.NotifyOnFinance);
    }

    public Task NotifyBookingExpiredAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime)
    {
        var title = "⚠️ Зустріч скасовано системою";
        var msg = $"Експерт {expertFirstName} {expertLastName} не підтвердив вашу зустріч на {startTime:dd.MM HH:mm} (UTC). Зайдіть у деталі зустрічі, щоб повернути свої кошти.";
        return DispatchNotificationAsync(studentId, title, msg, "BookingExpired", s => s.NotifyOnBooking);
    }

    public Task NotifyMeetingResolvedByStudentAsync(Guid expertId, string studentFirstName, string studentLastName, DateTime startTime, decimal amount, string currency, bool isSuccessful)
    {
        string timeString = startTime.ToString("dd.MM.yyyy HH:mm");
        var title = isSuccessful ? "✅ Зустріч успішно завершена!" : "⚠️ Зустріч скасована";
        var msg = isSuccessful
            ? $"Студент {studentFirstName} {studentLastName} підтвердив проведення зустрічі ({timeString} UTC).\n💸 {amount:0.####} {currency} відправлено на ваш гаманець."
            : $"Студент {studentFirstName} {studentLastName} вказав, що зустріч ({timeString} UTC) не відбулася. Кошти повернуті студенту.";

        return DispatchNotificationAsync(expertId, title, msg, "MeetingResolved", s => s.NotifyOnBooking);
    }

    public Task NotifyMeetingAutoResolvedAsync(Guid expertId, string studentFirstName, string studentLastName, DateTime startTime, decimal amount, string currency, bool isSuccessful)
    {
        string timeString = startTime.ToString("dd.MM.yyyy HH:mm");
        var title = isSuccessful ? "✅ Зустріч успішно завершена!" : "⚠️ Зустріч скасована системою";
        var msg = isSuccessful
            ? $"💸 {amount:0.####} {currency} відправлено на ваш гаманець."
            : $"На основі даних Discord виявлено, що експерт був відсутній на зустрічі ({timeString} UTC). Кошти повернуті студенту.";

        return DispatchNotificationAsync(expertId, title, msg, "MeetingAutoResolved", s => s.NotifyOnBooking);
    }

    public Task NotifyStudentAutoRefundAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime, decimal amount, string currency)
    {
        string timeString = startTime.ToString("dd.MM.yyyy HH:mm");
        var title = "⚠️ Зустріч не відбулася";
        var msg = $"Система зафіксувала, що експерт {expertFirstName} {expertLastName} не з'явився на заплановану зустріч ({timeString} UTC) у Discord.\n\n💸 {amount:0.####} {currency} успішно повернуто на ваш гаманець.";
        return DispatchNotificationAsync(studentId, title, msg, "StudentAutoRefund", s => s.NotifyOnFinance);
    }

    public Task NotifyNewFeedbackAsync(Guid expertId, string studentFirstName, string studentLastName, decimal rating, string? comment)
    {
        var title = "⭐ Новий відгук!";
        var msg = $"Студент {studentFirstName} {studentLastName} залишив вам відгук.\nОцінка: {rating}/5\nКоментар: {comment ?? "Без коментаря"}";
        return DispatchNotificationAsync(expertId, title, msg, "NewFeedback", s => s.NotifyOnNewFeedback);
    }
}