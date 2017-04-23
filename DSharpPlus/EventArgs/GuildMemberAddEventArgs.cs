using System;

namespace DSharpPlus
{
    public class GuildMemberAddEventArgs : EventArgs
    {
        public DiscordMember Member { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Member.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
    }
}
