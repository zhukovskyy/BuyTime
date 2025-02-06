using BuyTime_Application.Common.Interfaces.IService;

namespace BuyTime_Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly string _telegramBotToken = "8143691513:AAF8fyc9l-TaaNHXx5VlX8npMz5UgVZjGK4"; 
    
    public async Task SendMessageAsync(string? chatId, string message)
    {
        var client = new HttpClient();
        var url = $"https://api.telegram.org/bot{_telegramBotToken}/sendMessage";
        var parameters = new Dictionary<string, string>
        {
            { "chat_id", chatId },
            { "text", message }
        };
        var content = new FormUrlEncodedContent(parameters);
    
        var response = await client.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();
    
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Telegram API error: {responseString}");
        }
    }

}