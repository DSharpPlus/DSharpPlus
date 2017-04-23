using System;

namespace DSharpPlus
{
    public class GuildRoleUpdateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuildAsync(GuildID).Result;
        public DiscordRole Role { get; internal set; }
        public DiscordRole RoleBefore { get; internal set; }
    }
}
