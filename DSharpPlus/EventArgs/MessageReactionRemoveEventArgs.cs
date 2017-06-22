namespace DSharpPlus
{
    public class MessageReactionRemoveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Message that got one of its reactions removed
        /// </summary>
        public DiscordMessage Message { get; internal set; }
        /// <summary>
        /// Channel this message belongs to
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        /// <summary>
        /// user whose reaction got removed
        /// </summary>
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Emoji user reacted with
        /// </summary>
        public DiscordEmoji Emoji { get; internal set; }

        public MessageReactionRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
