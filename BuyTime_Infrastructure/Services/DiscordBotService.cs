using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Infrastructure.Common.Settings;
using Discord;
using Discord.WebSocket;
using ErrorOr;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace BuyTime_Infrastructure.Services;

public class DiscordBotService : IHostedService, IDiscordService
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordSettings _settings;
    private readonly ILogger<DiscordBotService> _logger;
    private readonly ConcurrentDictionary<ulong, HashSet<ulong>> _activeMeetings = new();

    public DiscordBotService(IOptions<DiscordSettings> settings, ILogger<DiscordBotService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true
        };

        _client = new DiscordSocketClient(config);

        _client.Log += msg =>
        {
            _logger.LogInformation("[Discord] {Message}", msg.Message);
            return Task.CompletedTask;
        };

        _client.UserVoiceStateUpdated += (user, oldState, newState) =>
        {
            if (newState.VoiceChannel != null && _activeMeetings.TryGetValue(newState.VoiceChannel.Id, out var attendees))
            {
                lock (attendees) { attendees.Add(user.Id); }
                _logger.LogInformation("[ВХОД] {Username} joined '{ChannelName}'", user.Username, newState.VoiceChannel.Name);
            }

            if (oldState.VoiceChannel != null && _activeMeetings.ContainsKey(oldState.VoiceChannel.Id))
            {
                _logger.LogInformation("[ВЫХОД] {Username} left '{ChannelName}'", user.Username, oldState.VoiceChannel.Name);
            }
            return Task.CompletedTask;
        };

        _client.Ready += () =>
        {
            _logger.LogInformation("The bot has been successfully launched and is ready to work!");
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.LoginAsync(TokenType.Bot, _settings.BotToken);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }

    public async Task<ErrorOr<string>> CreateMeetingAsync(string topic, List<string> discordIds)
    {
        var targetIds = discordIds
            .Select(id => ulong.TryParse(id, out var uid) ? uid : 0)
            .Where(id => id != 0)
            .ToList();

        var guild = _client.GetGuild(_settings.GuildId);
        if (guild == null)
            return Error.Failure("Discord.GuildNotFound", "Discord server not found. Either the bot is offline or GuildId is incorrect.");

        try
        {
            var channel = await guild.CreateVoiceChannelAsync(topic);

            var allow = new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Allow);
            var deny = new OverwritePermissions(viewChannel: PermValue.Deny, connect: PermValue.Deny);

            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, deny);
            await channel.AddPermissionOverwriteAsync(_client.CurrentUser, allow);

            foreach (var userId in targetIds)
            {
                var targetUser = guild.GetUser(userId);
                if (targetUser != null)
                {
                    await channel.AddPermissionOverwriteAsync(targetUser, allow);
                }
            }

            _activeMeetings.TryAdd(channel.Id, new HashSet<ulong>());
            var invite = await channel.CreateInviteAsync(maxAge: 0);

            return invite.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Discord channel");
            return Error.Failure("Discord.CreateChannelFailed", ex.Message);
        }
    }

    public async Task FinishMeetingAsync(ulong channelId)
    {
        if (_activeMeetings.TryRemove(channelId, out var finalAttendees))
        {
            var guild = _client.GetGuild(_settings.GuildId);
            var channel = guild?.GetVoiceChannel(channelId);

            if (channel != null)
            {
                await channel.DeleteAsync();
            }

            _logger.LogInformation("Meeting finished. Unique participants: {Count}", finalAttendees.Count);
        }
    }
}