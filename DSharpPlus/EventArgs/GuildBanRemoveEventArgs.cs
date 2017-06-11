namespace DSharpPlus
{
    public class GuildBanRemoveEventArgs : DiscordEventArgs
    {
        public DiscordMember Member { get; internal set; }
        public DiscordGuild Guild { get; internal set; }

        public GuildBanRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
