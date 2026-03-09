using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IService;

public interface IDiscordService
{
    Task<ErrorOr<string>> CreateMeetingAsync(string topic, List<string> discordIds);
    Task FinishMeetingAsync(ulong channelId);
}