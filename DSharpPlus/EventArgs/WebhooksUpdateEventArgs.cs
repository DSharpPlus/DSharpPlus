using System;

namespace DSharpPlus
{
    public class WebhooksUpdateEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public ulong ChannelID { get; internal set; }
        public DiscordChannel Channel => this.Discord._rest_client.InternalGetChannel(ChannelID).GetAwaiter().GetResult();
    }
}
