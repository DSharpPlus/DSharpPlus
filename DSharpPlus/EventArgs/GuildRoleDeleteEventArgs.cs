using System;

namespace DSharpPlus
{
    public class GuildRoleDeleteEventArgs : EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public ulong RoleID;
    }
}
