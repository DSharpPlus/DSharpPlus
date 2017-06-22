using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// New message
        /// </summary>
        public DiscordMessage Message { get; internal set; }
        /// <summary>
        /// Message's channel
        /// </summary>
        public DiscordChannel Channel => Message.Channel;
        /// <summary>
        /// Message's Guild
        /// </summary>
        public DiscordGuild Guild => Channel.Guild;
        /// <summary>
        /// Message's Author
        /// </summary>
        public DiscordUser Author => Message.Author;

        /// <summary>
        /// Mentioned users
        /// </summary>
        public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }
        /// <summary>
        /// Mentioned roles
        /// </summary>
        public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }
        /// <summary>
        /// Mentioned channels
        /// </summary>
        public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }

        public MessageCreateEventArgs(DiscordClient client) : base(client) { }
    }
}