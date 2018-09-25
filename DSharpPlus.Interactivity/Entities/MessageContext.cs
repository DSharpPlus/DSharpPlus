using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity
{
    public class MessageContext
    {
        /// <summary>
        /// Message that was found
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// User that was found
        /// </summary>
        public DiscordUser User 
            => Message.Author;

        /// <summary>
        /// Channel that was found
        /// </summary>
        public DiscordChannel Channel 
            => Message.Channel;

        /// <summary>
        /// Guild that was found
        /// </summary>
        public DiscordGuild Guild 
            => Channel.Guild;

        /// <summary>
        /// Interactivity extension that found this context
        /// </summary>
        public InteractivityExtension Interactivity { get; internal set; }

        /// <summary>
        /// Discordclient this context belongs to
        /// </summary>
        public DiscordClient Client 
            => Interactivity.Client;

        /// <summary>
        /// Channels that were mentioned
        /// </summary>
        public IReadOnlyList<DiscordChannel> MentionedChannels 
            => Message.MentionedChannels;

        /// <summary>
        /// Roles that were mentioned
        /// </summary>
        public IReadOnlyList<DiscordRole> MentionedRoles 
            => Message.MentionedRoles;

        /// <summary>
        /// Users that were mentioned
        /// </summary>
        public IReadOnlyList<DiscordUser> MentionedUsers 
            => Message.MentionedUsers;
    }
}
