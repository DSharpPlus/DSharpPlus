namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).Result;
        public DiscordRole Role { get; internal set; }

        public GuildRoleCreateEventArgs(DiscordClient client) : base(client) { }
    }
}
