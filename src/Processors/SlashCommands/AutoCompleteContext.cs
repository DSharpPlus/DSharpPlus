using System.Collections.Generic;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public sealed record AutoCompleteContext : AbstractContext
    {
        public required DiscordInteraction Interaction { get; init; }
        public required IEnumerable<DiscordInteractionDataOption> Options { get; init; }
        public required IReadOnlyDictionary<CommandParameter, object?> Arguments { get; init; }
        public required CommandParameter AutoCompleteArgument { get; init; }
        public required object UserInput { get; init; }
    }
}
