namespace DSharpPlus
{
    public class MessageReactionRemoveAllEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Message that got its reactions removed
        /// </summary>
        public DiscordMessage Message { get; internal set; }
        /// <summary>
        /// Channel this message belongs to
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        public MessageReactionRemoveAllEventArgs(DiscordClient client) : base(client) { }
    }
}
