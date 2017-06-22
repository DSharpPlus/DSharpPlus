namespace DSharpPlus
{
    public class MessageDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Message that got deleted
        /// </summary>
        public DiscordMessage Message { get; internal set; }
        /// <summary>
        /// Channel this message belonged to
        /// </summary>
        public DiscordChannel Channel { get; internal set; }
        /// <summary>
        /// Guild this message was sent in
        /// </summary>
        public DiscordGuild Guild => this.Channel.Guild;

        public MessageDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
