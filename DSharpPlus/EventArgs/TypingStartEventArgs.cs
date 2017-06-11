using System;

namespace DSharpPlus
{
    public class TypingStartEventArgs : DiscordEventArgs
    {
        public DiscordChannel Channel { get; internal set; }
        public DiscordUser User { get; internal set; }
        public DateTimeOffset StartedAt { get; internal set; }

        public TypingStartEventArgs(DiscordClient client) : base(client) { }
    }
}
