using System.Collections.Generic;

using DSharpPlus.Commands.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public record InteractionConverterContext : ConverterContext
{
    public required DiscordInteraction Interaction { get; init; }
    public required IReadOnlyList<DiscordInteractionDataOption> Options { get; init; }
    public override DiscordInteractionDataOption Argument => this.Options[this.ParameterIndex];
    public int ArgumentIndex { get; private set; } = -1;

    /// <inheritdoc/>
    public override bool NextParameter() 
        => this.Interaction.Data.Options is not null && base.NextParameter();

    public override bool NextArgument()
    {
        if (this.ArgumentIndex + 1 >= this.Options.Count)
        {
            return false;
        }

        this.ArgumentIndex++;
        return true;
    }
}
