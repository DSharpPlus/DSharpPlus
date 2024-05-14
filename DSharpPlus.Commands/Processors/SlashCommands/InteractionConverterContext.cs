using System.Collections.Generic;
using System.Linq;

using DSharpPlus.Commands.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

/// <summary>
/// Represents a context for interaction-based argument converters.
/// </summary>
public record InteractionConverterContext : ConverterContext
{
    /// <summary>
    /// The underlying interaction.
    /// </summary>
    public required DiscordInteraction Interaction { get; init; }

    /// <summary>
    /// The options passed to this command.
    /// </summary>
    public required IReadOnlyList<DiscordInteractionDataOption> Options { get; init; }

    /// <summary>
    /// The current argument to convert.
    /// </summary>
    public override DiscordInteractionDataOption? Argument 
        => this.Options.SingleOrDefault(x => x.Name == this.Parameter.Name);

    /// <inheritdoc/>
    public override bool NextParameter() 
        => this.Interaction.Data.Options is not null && base.NextParameter();
}
