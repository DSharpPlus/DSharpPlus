using System;

namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.User.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public DiscordUser User { get; internal set; }
    }
}
