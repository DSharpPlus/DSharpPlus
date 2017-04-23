using System;

namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Role.Discord._rest_client.InternalGetGuildAsync(GuildID).Result;
        public DiscordRole Role { get; internal set; }
    }
}
