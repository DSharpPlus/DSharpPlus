using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public IReadOnlyList<DiscordMember> Members { get; internal set; }

        public GuildMembersChunkEventArgs(DiscordClient client) : base(client) { }
    }
}
