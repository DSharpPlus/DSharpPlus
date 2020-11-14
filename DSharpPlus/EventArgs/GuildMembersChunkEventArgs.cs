﻿using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildMembersChunked"/> event.
    /// </summary>
    public class GuildMembersChunkEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild that requested this chunk.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the collection of members returned from this chunk.
        /// </summary>
        public IReadOnlyCollection<DiscordMember> Members { get; internal set; }

        /// <summary>
        /// Gets the current chunk index from the response.
        /// </summary>
        public int ChunkIndex { get; internal set; }

        /// <summary>
        /// Gets the total amount of chunks for the request. 
        /// </summary>
        public int ChunkCount { get; internal set; }

        /// <summary>
        /// Gets the collection of presences returned from this chunk, if specified.
        /// </summary>
        public IReadOnlyCollection<DiscordPresence> Presences { get; internal set; }

        /// <summary>
        /// Gets the returned Ids that were not found in the chunk, if specified.
        /// </summary>
        public IReadOnlyCollection<ulong> NotFound { get; internal set; }

        /// <summary>
        /// Gets the unique string used to identify the request, if specified.
        /// </summary>
        public string Nonce { get; set; }

        internal GuildMembersChunkEventArgs(DiscordClient client) : base(client) { }
    }
}
