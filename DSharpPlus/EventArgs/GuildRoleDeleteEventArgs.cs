using System;

namespace DSharpPlus
{
    public class GuildRoleDeleteEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public ulong RoleID { get; internal set; }
    }
}
