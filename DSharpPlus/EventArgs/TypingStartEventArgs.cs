using System;

namespace DSharpPlus
{
    public class TypingStartEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Channel user started typing in
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        /// <summary>
        /// User that started typing
        /// </summary>
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// When this user started typing
        /// </summary>
        public DateTimeOffset StartedAt { get; internal set; }

        public TypingStartEventArgs(DiscordClient client) : base(client) { }
    }
}
