using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Message that got updated
        /// </summary>
        public DiscordMessage Message { get; internal set; }
        /// <summary>
        /// Channel this message belongs to
        /// </summary>
        public DiscordChannel Channel => Message.Channel;
        /// <summary>
        /// Guild this message was sent in
        /// </summary>
        public DiscordGuild Guild => Channel.Guild;
        /// <summary>
        /// This message's author
        /// </summary>
        public DiscordUser Author => Message.Author;

        /// <summary>
        /// Users that got mentioned
        /// </summary>
        public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }
        /// <summary>
        /// Roles that got mentioned
        /// </summary>
        public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }
        /// <summary>
        /// Channels that got mentioned
        /// </summary>
        public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }

        public MessageUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}