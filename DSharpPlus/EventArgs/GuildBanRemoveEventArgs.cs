using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildBanRemoveEventArgs : System.EventArgs
    {
        public DiscordUser User;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
