using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Infrastructure.Common.Settings;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace BuyTime_Infrastructure.Services;

public class ZoomService : IZoomService
{
    private readonly HttpClient _httpClient;
    private readonly ZoomSettings _settings;
    private const string ZoomApiBaseUrl = "https://api.zoom.us/v2";

    public ZoomService(HttpClient httpClient, IOptions<ZoomSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<ErrorOr<string>> CreateMeetingAsync(string topic, DateTime startTime, int durationMinutes)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
                return Error.Failure("ZoomAuthError", "Failed to get access token.");

            return await CreateMeetingInternal("me", accessToken, topic, startTime, durationMinutes);
        }
        catch (Exception ex)
        {
            return Error.Failure("ZoomException", ex.Message);
        }
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        var authBytes = Encoding.ASCII.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}");
        var authString = Convert.ToBase64String(authBytes);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://zoom.us/oauth/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "account_credentials"),
            new KeyValuePair<string, string>("account_id", _settings.AccountId)
        });

        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("access_token").GetString();
    }

    private async Task<ErrorOr<string>> CreateMeetingInternal(string userId, string accessToken, string topic, DateTime startTime, int durationMinutes)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ZoomApiBaseUrl}/users/{userId}/meetings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var payload = new
        {
            topic = topic,
            type = 2, // Scheduled
            start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            duration = durationMinutes,
            timezone = "UTC",
            settings = new
            {
                host_video = true,
                participant_video = true,
                join_before_host = true,
                approval_type = 2,
                auto_recording = "none"
            }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return Error.Failure("ZoomApiError", $"Status: {response.StatusCode}. {err}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (json.TryGetProperty("join_url", out var url))
        {
            return url.GetString() ?? string.Empty;
        }

        return Error.Failure("ZoomParseError", "No join_url in response");
    }
}