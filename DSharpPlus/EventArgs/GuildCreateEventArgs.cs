namespace DSharpPlus
{
    public class GuildCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that got created
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
