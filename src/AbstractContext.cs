using DSharpPlus.CommandAll.Commands;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll
{
    public abstract record AbstractContext
    {
        public required DiscordUser User { get; init; }
        public required DiscordChannel Channel { get; init; }
        public required CommandAllExtension Extension { get; init; }
        public required Command Command { get; init; }

        public DiscordGuild? Guild => Channel.Guild;
        public DiscordMember? Member => User as DiscordMember;
        public DiscordClient Client => Extension.Client;
    }
}
