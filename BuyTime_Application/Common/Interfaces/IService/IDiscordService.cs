using ErrorOr;

public record DiscordChannelResult(string InviteUrl, ulong ChannelId);

public interface IDiscordService
{
    Task<ErrorOr<DiscordChannelResult>> CreateMeetingAsync(string topic, List<string> discordIds);
    Task FinishMeetingAsync(ulong channelId);
    Task<bool> IsMeetingEmptyAsync(ulong channelId);
}