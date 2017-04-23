namespace DSharpPlus
{
    public class GuildMemberAddEventArgs : DiscordEventArgs
    {
        public DiscordMember Member { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();

        public GuildMemberAddEventArgs(DiscordClient client) : base(client) { }
    }
}
