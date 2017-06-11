namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }
        public DiscordMember Member { get; internal set; }

        public GuildMemberRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
