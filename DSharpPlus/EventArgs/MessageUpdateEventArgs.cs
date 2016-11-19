using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageUpdateEventArgs : EventArgs
    {
        public DiscordMessage Message;
        public List<DiscordMember> MentionedUsers;
        public List<DiscordRole> MentionedRoles;
        public List<DiscordChannel> MentionedChannels;
        public List<DiscordEmoji> UsedEmojis;
        public DiscordChannel Channel => Message.Parent;
        public DiscordGuild Guild => Channel.Parent;
    }
}