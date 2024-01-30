namespace DSharpPlus.Commands.Processors.SlashCommands;

using System.Collections.Generic;
using DSharpPlus.Commands.Commands;
using DSharpPlus.Entities;

public sealed record AutoCompleteContext : AbstractContext
{
    public required DiscordInteraction Interaction { get; init; }
    public required IEnumerable<DiscordInteractionDataOption> Options { get; init; }
    public required IReadOnlyDictionary<CommandParameter, object?> Arguments { get; init; }
    public required CommandParameter AutoCompleteArgument { get; init; }
    public required object UserInput { get; init; }
}
