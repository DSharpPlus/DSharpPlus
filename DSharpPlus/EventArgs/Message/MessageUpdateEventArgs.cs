using System.Collections.Generic;
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
        /// Gets the message before it got updated. This property will be null if the message was not cached.
        /// </summary>
        public DiscordMessage MessageBefore { get; internal set; }

        /// <summary>
        /// Gets the channel this message belongs to.
        /// </summary>
        public DiscordChannel Channel
            => this.Message.Channel;

        /// <summary>
        /// Gets the guild this message belongs to.
        /// </summary>
        public DiscordGuild Guild
            => this.Channel.Guild;

        /// <summary>
        /// Gets the author of the message.
        /// </summary>
        public DiscordUser Author
            => this.Message.Author;

        /// <summary>
        /// Gets the collection of mentioned users.
        /// </summary>
        public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }

        /// <summary>
        /// Gets the collection of mentioned roles.
        /// </summary>
        /// <remarks>
        /// Only shows the mentioned roles from <see cref="DiscordClient.MessageCreated" />. EDITS ARE NOT INCLUDED.
        /// </remarks>
        public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }

        /// <summary>
        /// Gets the collection of mentioned channels.
        /// </summary>
        public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }

        internal MessageUpdateEventArgs() : base() { }
    }
}
