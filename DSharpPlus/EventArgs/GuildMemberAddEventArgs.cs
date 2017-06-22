namespace DSharpPlus
{
    public class GuildMemberAddEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Member that got added
        /// </summary>
        public DiscordMember Member { get; internal set; }
        /// <summary>
        /// Guild member was added to
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildMemberAddEventArgs(DiscordClient client) : base(client) { }
    }
}
