namespace DSharpPlus
{
    public class UserSettingsUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// User whose settings got updated
        /// </summary>
        public DiscordUser User { get; internal set; }

        public UserSettingsUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
