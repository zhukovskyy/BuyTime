namespace BuyTime_Application.Common.Interfaces.IService;

public interface ITelegramService
{
    Task SendMessageAsync(string? chatId, string message);

    Task NotifyBookingCreatedAsync(Guid expertId, string studentFirstName, string studentLastName, DateTime startTime);
    Task NotifyBookingCancelledAsync(Guid targetUserId, string cancelledByRole, string cancelledByName, DateTime startTime, string reason, decimal? refundAmount = null, string? currency = null);
    Task NotifyBookingRejectedAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime);
    Task NotifyBookingConfirmedAsync(Guid studentId, string expertFirstName, string expertLastName, DateTime startTime, string? meetingLink);
    Task NotifyRefundReceivedAsync(Guid studentId, decimal amount, string currency);
    Task NotifyNewFeedbackAsync(Guid expertId, string studentFirstName, string studentLastName, decimal rating, string? comment);
}