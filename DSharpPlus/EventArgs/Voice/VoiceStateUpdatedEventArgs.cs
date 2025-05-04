using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for VoiceStateUpdated event.
/// </summary>
public class VoiceStateUpdatedEventArgs : DiscordEventArgs
{
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

    internal VoiceStateUpdatedEventArgs() : base() { }
}
