namespace DSharpPlus
{
    public class UserUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// User after it got updated
        /// </summary>
        public DiscordUser UserAfter { get; internal set; }
        /// <summary>
        /// User before it got updated
        /// </summary>
        public DiscordUser UserBefore { get; internal set; }

        public UserUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
