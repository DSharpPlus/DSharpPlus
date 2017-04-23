namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public DiscordUser User { get; internal set; }

        public GuildMemberRemoveEventArgs(DiscordClient client) : base(client) { }
    }
}
