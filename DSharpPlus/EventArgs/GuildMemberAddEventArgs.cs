using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildMemberAddEventArgs : System.EventArgs
    {
        public DiscordMember Member;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
