﻿using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.MessageUpdated"/> event.
    /// </summary>
    public class MessageUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the message that was updated.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Message before it got updated. Value may not be present if message was not cached.
        /// </summary>
        public Optional<DiscordMessage> MessageBefore { get; internal set; }

        /// <summary>
        /// Gets the channel this message belongs to.
        /// </summary>
        public DiscordChannel Channel 
            => Message.Channel;

        /// <summary>
        /// Gets the guild this message belongs to.
        /// </summary>
        public DiscordGuild Guild 
            => Channel.Guild;

        /// <summary>
        /// Gets the author of the message.
        /// </summary>
        public DiscordUser Author 
            => Message.Author;

        /// <summary>
        /// Gets the collection of mentioned users.
        /// </summary>
        public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }

        /// <summary>
        /// Gets the collection of mentioned roles.
        /// </summary>
        public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }

        /// <summary>
        /// Gets the collection of mentioned channels.
        /// </summary>
        public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }

        internal MessageUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}