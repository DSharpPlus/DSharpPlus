using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity
{
    public class MessageContext
    {
        public DiscordMessage Message;

        public DiscordUser User => Message.Author;

        public DiscordChannel Channel => Message.Channel;

        public DiscordGuild Guild => Channel.Guild;

        public InteractivityModule Interactivity;

        public DiscordClient Client => Interactivity.Client;

        public IReadOnlyList<DiscordChannel> MentionedChannels => Message.MentionedChannels;

        public IReadOnlyList<DiscordRole> MentionedRoles => Message.MentionedRoles;

        public IReadOnlyList<DiscordUser> MentionedUsers => Message.MentionedUsers;
    }
}
