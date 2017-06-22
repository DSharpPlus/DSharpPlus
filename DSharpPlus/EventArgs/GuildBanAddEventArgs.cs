namespace DSharpPlus
{
    public class GuildBanAddEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Member that got banned
        /// </summary>
        public DiscordMember Member { get; internal set; }
        /// <summary>
        /// Guild this member was banned in
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildBanAddEventArgs(DiscordClient client) : base(client) { }
    }
}
