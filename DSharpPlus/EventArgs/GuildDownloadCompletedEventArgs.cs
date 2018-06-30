using DSharpPlus.Entities;
using System.Collections.Generic;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildDownloadCompleted"/> event.
    /// </summary>
    public class GuildDownloadCompletedEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the dictionary of guilds that just finished downloading.
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordGuild> Guilds => Client.Guilds;

        internal GuildDownloadCompletedEventArgs(DiscordClient client) : base(client) { }
    }
}
