using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public IReadOnlyList<DiscordMember> Members { get; internal set; }

        public GuildMembersChunkEventArgs(DiscordClient client) : base(client) { }
    }
}
