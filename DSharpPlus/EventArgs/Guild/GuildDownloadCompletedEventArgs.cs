using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildDownloadCompleted"/> event.
/// </summary>
public class GuildDownloadCompletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the dictionary of guilds that just finished downloading.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

    internal GuildDownloadCompletedEventArgs(IReadOnlyDictionary<ulong, DiscordGuild> guilds)
        : base() => Guilds = guilds;
}
