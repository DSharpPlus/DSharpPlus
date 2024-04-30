namespace DSharpPlus.EventArgs;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.VoiceStateUpdated"/> event.
/// </summary>
public class VoiceStateUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the user whose voice state was updated.
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the related voice channel.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

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

    internal VoiceStateUpdateEventArgs() : base() { }
}
