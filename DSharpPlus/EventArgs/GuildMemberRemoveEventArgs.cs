namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild member was removed from
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Member that got removed
        /// </summary>
        public DiscordMember Member { get; internal set; }

        public GuildMemberRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
