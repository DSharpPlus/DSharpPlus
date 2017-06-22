namespace DSharpPlus
{
    public class GuildDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that got deleted
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Whether this guild is unavailable
        /// </summary>
        public bool Unavailable { get; internal set; }

        public GuildDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
