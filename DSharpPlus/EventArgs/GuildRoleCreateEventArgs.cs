using System;

namespace DSharpPlus
{
    public class GuildRoleCreateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordRole Role { get; internal set; }
    }
}
