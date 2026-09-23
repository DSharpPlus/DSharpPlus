using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Voice;

// handles bitrate updates from CHANNEL_UPDATE and channel moves from VOICE_STATE_UPDATE
internal sealed class GuildMonitoringEventHandler
    : IEventHandler<ChannelUpdatedEventArgs>,
    IEventHandler<VoiceStateUpdatedEventArgs>
{
    private readonly IVoiceConnectionRepository connectionRepository;

    public GuildMonitoringEventHandler(IVoiceConnectionRepository connectionRepository)
        => this.connectionRepository = connectionRepository;

    public Task HandleEventAsync(DiscordClient sender, ChannelUpdatedEventArgs eventArgs)
    {
        if (this.connectionRepository.Connections.TryGetValue(eventArgs.Guild.Id, out VoiceConnection? connection)
            && connection.ChannelId == eventArgs.ChannelAfter.Id
            && eventArgs.ChannelAfter.Bitrate is not null)
        {
            connection.SetChannelMaxBitrate(eventArgs.ChannelAfter.Bitrate.Value);
        }

        return Task.CompletedTask;
    }

    public async Task HandleEventAsync(DiscordClient sender, VoiceStateUpdatedEventArgs eventArgs)
    {
        if (eventArgs is { GuildId: ulong guildId, ChannelId: ulong channelId }
            && this.connectionRepository.Connections.TryGetValue(guildId, out VoiceConnection? connection)
            && connection.ChannelId != channelId // without this check, it takes the initial join as a move and crash-loops
            && sender.CurrentUser.Id == eventArgs.UserId)
        {
            DiscordGuild guild = await eventArgs.GetGuildAsync();
            await connection.MoveChannelAsync(channelId, guild.VoiceStates.Where(x => x.Value.ChannelId == channelId).Select(x => x.Value.UserId));
        }
    }
}
