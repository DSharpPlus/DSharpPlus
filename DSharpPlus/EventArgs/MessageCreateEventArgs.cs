using System.Collections.Generic;

namespace DSharpPlus
{
    public class MessageCreateEventArgs : DiscordEventArgs
    {
        public DiscordMessage Message { get; internal set; }
        public IReadOnlyList<DiscordMember> MentionedUsers { get; internal set; }
        public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }
        public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }
        public IReadOnlyList<DiscordEmoji> UsedEmojis { get; internal set; }
        public DiscordChannel Channel => Message.Channel;
        public DiscordGuild Guild => Channel.Guild;
        public DiscordUser Author => Message.Author;

        public MessageCreateEventArgs(DiscordClient client) : base(client) { }
    }
}