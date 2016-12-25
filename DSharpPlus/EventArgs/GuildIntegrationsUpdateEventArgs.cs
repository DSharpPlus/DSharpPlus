using System;

namespace DSharpPlus
{
    public class GuildIntegrationsUpdateEventArgs : EventArgs
    {
        internal ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
