using System.Collections.Generic;
using System.Linq;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public sealed record SlashConverterContext : ConverterContext
    {
        public required DiscordInteraction Interaction { get; init; }
        public required IEnumerable<DiscordInteractionDataOption> Options { get; init; }
        public DiscordInteractionDataOption CurrentOption => Options.ElementAt(ArgumentIndex);
    }
}
