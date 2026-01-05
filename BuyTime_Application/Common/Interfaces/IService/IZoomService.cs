using ErrorOr;

namespace BuyTime_Application.Common.Interfaces.IService;

public interface IZoomService
{
    Task<ErrorOr<string>> CreateMeetingAsync(string topic, DateTime startTime, int durationMinutes);
}