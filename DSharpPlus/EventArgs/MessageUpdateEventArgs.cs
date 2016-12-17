using System.Collections.Generic;
using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class MessageUpdateEventArgs : System.EventArgs
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