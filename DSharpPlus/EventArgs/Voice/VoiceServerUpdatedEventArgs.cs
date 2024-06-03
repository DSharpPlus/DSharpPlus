using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.VoiceServerUpdated"/> event.
/// </summary>
public class VoiceServerUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild for which the update occurred.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the new voice endpoint.
    /// </summary>
    public string Endpoint { get; internal set; }

    /// <summary>
    /// Gets the voice connection token.
    /// </summary>
    public string VoiceToken { get; internal set; }

    internal VoiceServerUpdatedEventArgs() : base() { }
}
