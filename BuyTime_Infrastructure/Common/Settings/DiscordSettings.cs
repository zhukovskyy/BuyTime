namespace BuyTime_Infrastructure.Common.Settings;

public class DiscordSettings
{
    public const string SectionName = "Discord";
    public string BotToken { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
}