using System;

namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuildAsync(GuildID).Result;
        public DiscordRole Role { get; internal set; }
    }
}
