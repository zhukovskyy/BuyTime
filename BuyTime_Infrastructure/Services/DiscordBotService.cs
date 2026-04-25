using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Domain.Entities;
using BuyTime_Domain.Enums;
using BuyTime_Infrastructure.Common.Persistence;
using BuyTime_Infrastructure.Common.Settings;
using Discord;
using Discord.WebSocket;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuyTime_Infrastructure.Services;

public class DiscordBotService : IHostedService, IDiscordService
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordSettings _settings;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
        IOptions<DiscordSettings> settings,
        ILogger<DiscordBotService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;

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

        _client.UserVoiceStateUpdated += async (user, oldState, newState) =>
        {
            if (newState.VoiceChannel != null)
            {
                await RecordAttendanceInDbAsync(newState.VoiceChannel.Id, user.Id, newState.VoiceChannel.Name, user.Username);
            }
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

    public async Task<ErrorOr<DiscordChannelResult>> CreateMeetingAsync(string topic, List<string> discordIds)
    {
        var targetIds = discordIds
            .Select(id => ulong.TryParse(id, out var uid) ? uid : 0)
            .Where(id => id != 0)
            .ToList();

        var guild = _client.GetGuild(_settings.GuildId);
        if (guild == null)
            return Error.Failure("Discord.GuildNotFound", "Discord server not found.");

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

            var invite = await channel.CreateInviteAsync(maxAge: 0);
            return new DiscordChannelResult(invite.Url, channel.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Discord channel");
            return Error.Failure("Discord.CreateChannelFailed", ex.Message);
        }
    }

    public async Task FinishMeetingAsync(ulong channelId)
    {
        try
        {
            var guild = _client.GetGuild(_settings.GuildId);
            if (guild != null)
            {
                var channel = guild.GetVoiceChannel(channelId);
                if (channel != null)
                {
                    await channel.DeleteAsync();
                    _logger.LogInformation("Discord channel {ChannelId} successfully deleted.", channelId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while trying to delete Discord channel {ChannelId}", channelId);
        }
    }

    public Task<bool> IsMeetingEmptyAsync(ulong channelId)
    {
        var guild = _client.GetGuild(_settings.GuildId);
        var channel = guild?.GetVoiceChannel(channelId);

        if (channel == null) return Task.FromResult(true);

        return Task.FromResult(channel.ConnectedUsers.Count == 0);
    }

    private async Task RecordAttendanceInDbAsync(ulong channelId, ulong discordUserId, string channelName, string username)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BuyTimeDbContext>();

            string channelIdStr = channelId.ToString();

            var marker = await dbContext.MeetingAttendances
                .FirstOrDefaultAsync(ma => ma.ExternalMeetingId == channelIdStr && ma.ExternalUserId == 0);

            if (marker == null) return;

            string discordUserIdStr = discordUserId.ToString();

            bool alreadyRecorded = await dbContext.MeetingAttendances
                .AnyAsync(ma => ma.ExternalMeetingId == channelIdStr && ma.ExternalUserId == discordUserId);

            if (!alreadyRecorded)
            {
                _logger.LogInformation("[ВХОД] {Username} joined '{ChannelName}'", username, channelName);

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == discordUserIdStr);

                var attendance = new MeetingAttendance
                {
                    Id = Guid.NewGuid(),
                    BookingId = marker.BookingId,
                    ExternalMeetingId = channelIdStr,
                    Platform = MeetingPlatform.Discord,
                    ExternalUserId = discordUserId,
                    SystemUserId = user?.Id,
                    FirstJoinedAt = DateTime.UtcNow
                };

                dbContext.MeetingAttendances.Add(attendance);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save attendance for channel {ChannelId}, user {UserId}", channelId, discordUserId);
        }
    }
}