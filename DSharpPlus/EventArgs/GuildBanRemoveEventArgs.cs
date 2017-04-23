using System;

namespace DSharpPlus
{
    public class GuildBanRemoveEventArgs : EventArgs
    {
        public DiscordUser User { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuildAsync(GuildID).Result;
    }
}
