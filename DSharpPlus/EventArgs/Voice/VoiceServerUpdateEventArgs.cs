using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

using Caching;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.VoiceServerUpdated"/> event.
/// </summary>
public class VoiceServerUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild for which the update occurred.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }

    /// <summary>
    /// Gets the new voice endpoint.
    /// </summary>
    public string Endpoint { get; internal set; }

    /// <summary>
    /// Gets the voice connection token.
    /// </summary>
    public string VoiceToken { get; internal set; }

    internal VoiceServerUpdateEventArgs() : base() { }
}
