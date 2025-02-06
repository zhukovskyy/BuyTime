namespace BuyTime_Application.Common.Interfaces.IService;

public interface ITelegramService
{
    Task SendMessageAsync(string? chatId, string message);
}