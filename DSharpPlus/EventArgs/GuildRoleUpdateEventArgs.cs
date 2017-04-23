using System;

namespace DSharpPlus
{
    public class GuildRoleUpdateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Role.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public DiscordRole Role { get; internal set; }
        public DiscordRole RoleBefore { get; internal set; }
    }
}
