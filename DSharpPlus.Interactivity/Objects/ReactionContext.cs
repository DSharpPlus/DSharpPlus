using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    public class ReactionContext
    {
        public DiscordChannel Channel;

        public DiscordUser User;

        public DiscordEmoji Emoji;

        public DiscordMessage Message;

        public DiscordGuild Guild => Channel.Guild;

        public InteractivityModule Interactivity;

        public DiscordClient Client => Interactivity.Client;
    }
}
