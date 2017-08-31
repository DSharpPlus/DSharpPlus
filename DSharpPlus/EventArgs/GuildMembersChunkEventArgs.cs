using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildMembersChunked"/> event.
    /// </summary>
    public class GuildMembersChunkEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild whose members were requested.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets a collection containing the members in the received chunk.
        /// </summary>
        public IReadOnlyList<DiscordMember> Members { get; internal set; }

        internal GuildMembersChunkEventArgs(DiscordClient client) : base(client) { }
    }
}
