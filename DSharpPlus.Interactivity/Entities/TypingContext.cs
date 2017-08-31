using System;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity
{
    public class TypingContext
    {
        public DiscordUser User { get; internal set; }

        public DiscordChannel Channel { get; internal set; }

        public DateTimeOffset StartedAt { get; internal set; }

        public DiscordGuild Guild => Channel.Guild;

        public InteractivityModule Interactivity { get; internal set; }

        public DiscordClient Client => Interactivity.Client;
    }
}
