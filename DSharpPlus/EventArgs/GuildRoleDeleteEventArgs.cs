using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildRoleDeleteEventArgs : System.EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordRole Role;
    }
}
