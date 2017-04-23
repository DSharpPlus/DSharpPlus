namespace DSharpPlus
{
    public class GuildRoleDeleteEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public ulong RoleID { get; internal set; }

        public GuildRoleDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
