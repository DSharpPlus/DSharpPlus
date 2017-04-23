namespace DSharpPlus
{
    public class UserUpdateEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }
        public DiscordUser UserBefore { get; internal set; }

        public UserUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
