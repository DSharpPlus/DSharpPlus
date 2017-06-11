namespace DSharpPlus
{
    public class GuildBanAddEventArgs : DiscordEventArgs
    {
        public DiscordMember Member { get; internal set; }
        public DiscordGuild Guild { get; internal set; }

        public GuildBanAddEventArgs(DiscordClient client) : base(client) { }
    }
}
