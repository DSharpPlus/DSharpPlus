using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }
        public IReadOnlyList<DiscordMember> Members { get; internal set; }

        public GuildMembersChunkEventArgs(DiscordClient client) : base(client) { }
    }
}
