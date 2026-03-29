namespace BuyTime_Infrastructure.Common.Settings;

public class TelegramSettings
{
    public const string SectionName = "Telegram";
    public string BotToken { get; set; } = string.Empty;
}