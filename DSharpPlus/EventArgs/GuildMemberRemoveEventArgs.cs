using System;

namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuildAsync(GuildID).Result;
        public DiscordUser User { get; internal set; }
    }
}
