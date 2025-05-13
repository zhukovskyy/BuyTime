using BuyTime_Application.Common.Interfaces.IService;

namespace BuyTime_Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly string _telegramBotToken = "7606255333:AAEP4sV2SbKbaH08Tdd2iCW8msSHK9d9PVo";



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