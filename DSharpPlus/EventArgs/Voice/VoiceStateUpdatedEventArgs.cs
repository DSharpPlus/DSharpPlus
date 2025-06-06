using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for VoiceStateUpdated event.
/// </summary>
public class VoiceStateUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the ID of the user whose voice state was updated.
    /// </summary>
    public ulong UserId => this.After.UserId;

    /// <summary>
    /// Gets the ID of the channel this user is now connected to.
    /// </summary>
    public ulong? ChannelId => this.After.ChannelId;

    /// <summary>
    /// Gets the ID of the guild this voice state update is associated with.
    /// </summary>
    public ulong? GuildId => this.After.GuildId;

    /// <summary>
    /// Gets the member associated with this voice state.
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always fetch the member from the API.</param>
    /// <returns>Returns the member associated with this voice state. Null if the voice state is not associated with a guild.</returns>
    public async ValueTask<DiscordUser?> GetUserAsync(bool skipCache = false)
        => await this.After.GetUserAsync(skipCache);

    /// <summary>
    /// Gets the guild associated with this voice state.
    /// </summary>
    /// <returns>Returns the guild associated with this voicestate</returns>
    public async ValueTask<DiscordGuild?> GetGuildAsync(bool skipCache = false)
        => await this.After.GetGuildAsync(skipCache);

    /// <summary>
    /// Gets the channel associated with this voice state.
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always fetch the channel from the API.</param>
    /// <returns>Returns the channel associated with this voice state. Null if the voice state is not associated with a guild.</returns>
    public async ValueTask<DiscordChannel?> GetChannelAsync(bool skipCache = false)
        => await this.After.GetChannelAsync(skipCache);

    /// <summary>
    /// Gets the voice state pre-update.
    /// </summary>
    public DiscordVoiceState Before { get; internal set; }

    /// <summary>
    /// Gets the voice state post-update.
    /// </summary>
    public DiscordVoiceState After { get; internal set; }

    /// <summary>
    /// Gets the ID of voice session.
    /// </summary>
    public string SessionId { get; internal set; }

    internal VoiceStateUpdatedEventArgs() { }
}
