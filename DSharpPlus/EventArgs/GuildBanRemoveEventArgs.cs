namespace DSharpPlus
{
    public class GuildBanRemoveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Member that just got unbanned
        /// </summary>
        public DiscordMember Member { get; internal set; }
        /// <summary>
        /// Guild this member was unbanned in
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public GuildBanRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
