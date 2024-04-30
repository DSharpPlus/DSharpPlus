namespace DSharpPlus.EventArgs;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for UserSpeaking event.
/// </summary>
public class UserSpeakingEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the users whose speaking state changed.
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the SSRC of the audio source.
    /// </summary>
    public uint SSRC { get; internal set; }

    /// <summary>
    /// Gets whether this user is speaking.
    /// </summary>
    public bool Speaking { get; internal set; }

    internal UserSpeakingEventArgs() : base() { }
}
