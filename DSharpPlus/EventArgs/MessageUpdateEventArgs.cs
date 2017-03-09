using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageUpdateEventArgs : EventArgs
    {
        public DiscordMessage Message { get; internal set; }
        public IReadOnlyCollection<DiscordMember> MentionedUsers { get; internal set; }
        public IReadOnlyCollection<DiscordRole> MentionedRoles { get; internal set; }
        public IReadOnlyCollection<DiscordChannel> MentionedChannels { get; internal set; }
        public IReadOnlyCollection<DiscordEmoji> UsedEmojis { get; internal set; }
        public DiscordChannel Channel => Message.Parent;
        public DiscordGuild Guild => Channel.Parent;
    }
}