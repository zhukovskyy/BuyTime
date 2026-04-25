using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Infrastructure.Common.Persistence;
using BuyTime_Infrastructure.Common.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuyTime_Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly string _telegramBotToken;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramService> logger,
        IOptions<TelegramSettings> telegramSettings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _telegramBotToken = telegramSettings.Value.BotToken;
    }

    public async Task SendMessageAsync(string? chatId, string message)
    {
        if (string.IsNullOrEmpty(chatId)) return;

        try
        {
            var client = new HttpClient();
            var url = $"https://api.telegram.org/bot{_telegramBotToken}/sendMessage";
            var parameters = new Dictionary<string, string>
            {
                { "chat_id", chatId },
                { "text", message },
                { "parse_mode", "HTML" }
            };
            var content = new FormUrlEncodedContent(parameters);

            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Telegram API error: {responseString}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send Telegram message: {ex.Message}");
        }
    }

    private async Task TrySendNotificationAsync(Guid userId, string message, Func<BuyTime_Domain.Entities.UserSettings, bool> settingPredicate)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BuyTimeDbContext>();

            var user = await dbContext.Users
                .Include(u => u.Settings)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.TelegramChatId)) return;

            if (user.Settings != null)
            {
                if (!user.Settings.NotifyInTelegram) return;
                if (!settingPredicate(user.Settings)) return;
            }

            await SendMessageAsync(user.TelegramChatId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in TrySendNotificationAsync: {ex.Message}");
        }
    }

    public Task NotifyBookingCreatedAsync(Guid expertId, string studentFirstName, string studentLastName, DateTime startTime)
    {
        var msg = $"📅 <b>Нове бронювання!</b>\nСтудент <b>{studentFirstName} {studentLastName}</b> забронював зустріч на {startTime:dd.MM HH:mm} (UTC).";
        return TrySendNotificationAsync(expertId, msg, s => s.NotifyOnBooking);
    }

    public Task NotifyBookingCancelledAsync(Guid targetUserId, string cancelledByRole, string cancelledByName, DateTime startTime, string reason, decimal? refundAmount = null, string? currency = null)
    {
        var roleName = cancelledByRole.ToLower() == "student" ? "Студент" : "Експерт";

        var msg = $"⚠️ <b>Бронювання скасовано</b>\n{roleName} <b>{cancelledByName}</b> скасував зустріч на {startTime:dd.MM HH:mm} (UTC).\nПричина: {reason}";

        if (refundAmount.HasValue && !string.IsNullOrEmpty(currency))
        {
            msg += $"\n\n💸 <b>Повернення коштів:</b> {refundAmount.Value} {currency} успішно повернуто на ваш гаманець.";
        }

        return TrySendNotificationAsync(targetUserId, msg, s => s.NotifyOnBooking);
    }

    public Task NotifyBookingRejectedAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime)
    {
        var msg = $"❌ <b>Зустріч відхилено</b>\nЕксперт <b>{expertFirstName} {expertLastName}</b> не зміг підтвердити зустріч на {startTime:dd.MM HH:mm} (UTC). Ви можете повернути кошти в деталях цієї зустрічі.";
        return TrySendNotificationAsync(studentId, msg, s => s.NotifyOnBooking);
    }

    public async Task NotifyBookingConfirmedAsync(
    Guid studentId, string studentFirstName, string studentLastName,
    Guid expertId, string expertFirstName, string expertLastName,
    DateTime startTime, string? messageToStudent, string? meetingLink)
    {
        var linkText = string.IsNullOrEmpty(meetingLink) ? "" : $"Посилання: {meetingLink}";
        var expertMessageText = string.IsNullOrEmpty(messageToStudent) ? "" : messageToStudent;

        var studentMsg = $"✅ <b>Зустріч підтверджено!</b>\nЕксперт <b>{expertFirstName} {expertLastName}</b> підтвердив зустріч на {startTime:dd.MM HH:mm} (UTC).\nПовідомлення від експерта: {expertMessageText}\n{linkText}";

        var expertMsg = $"✅ <b>Ви підтвердили зустріч!</b>\nЗустріч зі студентом <b>{studentFirstName} {studentLastName}</b> на {startTime:dd.MM HH:mm} (UTC).\n{linkText}";

        var notifyStudentTask = TrySendNotificationAsync(studentId, studentMsg, s => s.NotifyOnBooking);
        var notifyExpertTask = TrySendNotificationAsync(expertId, expertMsg, s => s.NotifyOnBooking);

        await Task.WhenAll(notifyStudentTask, notifyExpertTask);
    }

    public Task NotifyRefundReceivedAsync(Guid studentId, decimal amount, string currency)
    {
        var msg = $"💸 <b>Повернення коштів</b>\nСума <b>{amount} {currency}</b> була успішно повернута на ваш гаманець.";
        return TrySendNotificationAsync(studentId, msg, s => s.NotifyOnFinance);
    }

    public Task NotifyBookingExpiredAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime)
    {
        var studentMsg = $"⚠️ <b>Зустріч скасовано системою</b>\nЕксперт <b>{expertFirstName} {expertLastName}</b> не підтвердив вашу зустріч на {startTime:dd.MM HH:mm} (UTC). Зайдіть у деталі зустрічі, щоб повернути свої кошти.";

        return TrySendNotificationAsync(studentId, studentMsg, s => s.NotifyOnBooking);
    }

    public Task NotifyMeetingResolvedByStudentAsync(
    Guid expertId,
    string studentFirstName,
    string studentLastName,
    DateTime startTime,
    decimal amount,
    string currency,
    bool isSuccessful)
    {
        string timeString = startTime.ToString("dd.MM.yyyy HH:mm");

        string msg = isSuccessful
            ? $"✅ <b>Зустріч успішно завершена!</b>\nСтудент <b>{studentFirstName} {studentLastName}</b> підтвердив проведення зустрічі ({timeString} UTC).\n💸 <b>{amount:0.####} {currency}</b> відправлено на ваш гаманець."
            : $"⚠️ <b>Зустріч скасована.</b>\nСтудент <b>{studentFirstName} {studentLastName}</b> вказав, що зустріч ({timeString} UTC) не відбулася. Кошти повернуті студенту.";

        return TrySendNotificationAsync(expertId, msg, s => s.NotifyOnBooking);
    }
    public Task NotifyNewFeedbackAsync(Guid expertId, string studentFirstName, string studentLastName, decimal rating, string? comment)
    {
        var msg = $"⭐ <b>Новий відгук!</b>\nСтудент <b>{studentFirstName} {studentLastName}</b> залишив вам відгук.\nОцінка: {rating}/5\nКоментар: {comment ?? "Без коментаря"}";
        return TrySendNotificationAsync(expertId, msg, s => s.NotifyOnNewFeedback);
    }
}