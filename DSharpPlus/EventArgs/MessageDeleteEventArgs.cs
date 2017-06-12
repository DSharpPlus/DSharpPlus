namespace DSharpPlus
{
    public class MessageDeleteEventArgs : DiscordEventArgs
    {
        public DiscordMessage Message { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordGuild Guild => this.Channel.Guild;

        public MessageDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
