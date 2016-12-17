using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildRoleUpdateEventArgs : System.EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordRole Role;
    }
}
