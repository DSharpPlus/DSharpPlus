using System;

namespace DSharpPlus
{
    public class GuildRoleDeleteEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuildAsync(GuildID).Result;
        public ulong RoleID { get; internal set; }
    }
}
