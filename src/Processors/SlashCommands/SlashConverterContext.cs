using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public sealed record SlashConverterContext : ConverterContext
    {
        public required DiscordInteraction Interaction { get; init; }
    }
}
