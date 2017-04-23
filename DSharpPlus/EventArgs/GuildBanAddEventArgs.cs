using System;

namespace DSharpPlus
{
    public class GuildBanAddEventArgs : EventArgs
    {
        public DiscordUser User { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.User.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
    }
}
