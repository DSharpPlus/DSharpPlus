namespace DSharpPlus
{
    public class UserSettingsUpdateEventArgs : DiscordEventArgs
    {
        public DiscordUser User { get; internal set; }

        public UserSettingsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
