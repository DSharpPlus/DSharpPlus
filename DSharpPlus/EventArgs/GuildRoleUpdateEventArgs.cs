using System;

namespace DSharpPlus
{
    public class GuildRoleUpdateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordRole Role { get; internal set; }
    }
}
