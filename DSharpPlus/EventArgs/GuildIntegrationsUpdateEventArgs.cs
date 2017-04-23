using System;

namespace DSharpPlus
{
    public class GuildIntegrationsUpdateEventArgs : EventArgs
    {
        internal ulong GuildID { get; set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuildAsync(GuildID).Result;
    }
}
