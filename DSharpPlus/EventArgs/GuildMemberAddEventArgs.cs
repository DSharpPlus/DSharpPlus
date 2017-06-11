namespace DSharpPlus
{
    public class GuildMemberAddEventArgs : DiscordEventArgs
    {
        public DiscordMember Member { get; internal set; }
        public DiscordGuild Guild { get; internal set; }

        public GuildMemberAddEventArgs(DiscordClient client) : base(client) { }
    }
}
