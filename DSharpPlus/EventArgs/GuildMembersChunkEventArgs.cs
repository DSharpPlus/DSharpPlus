using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild whose members were requested
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// New member list
        /// </summary>
        public IReadOnlyList<DiscordMember> Members { get; internal set; }

        public GuildMembersChunkEventArgs(DiscordClient client) : base(client) { }
    }
}
