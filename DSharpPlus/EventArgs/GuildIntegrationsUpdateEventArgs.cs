using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildIntegrationsUpdateEventArgs : System.EventArgs
    {
        internal ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
