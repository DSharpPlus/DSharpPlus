using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    public class TypingContext
    {
        public DiscordUser User;

        public DiscordChannel Channel;

        public DateTimeOffset StartedAt;

        public DiscordGuild Guild => Channel.Guild;

        public InteractivityModule Interactivity;

        public DiscordClient Client => Interactivity.Client;
    }
}
