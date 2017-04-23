using System;

namespace DSharpPlus
{
    public class GuildIntegrationsUpdateEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        internal ulong GuildID { get; set; }
        public DiscordGuild Guild => this.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
    }
}
