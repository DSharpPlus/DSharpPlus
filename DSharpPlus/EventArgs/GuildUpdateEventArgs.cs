namespace DSharpPlus
{
    public class GuildUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that got updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
