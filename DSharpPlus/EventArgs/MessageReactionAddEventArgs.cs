namespace DSharpPlus
{
    public class MessageReactionAddEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Message that got a new reaction
        /// </summary>
        public DiscordMessage Message { get; internal set; }
        /// <summary>
        /// Channel this message belongs to
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        /// <summary>
        /// User who reacted
        /// </summary>
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Emoji reacted with
        /// </summary>
        public DiscordEmoji Emoji { get; internal set; }

        public MessageReactionAddEventArgs(DiscordClient client) : base(client) { }
    }
}
