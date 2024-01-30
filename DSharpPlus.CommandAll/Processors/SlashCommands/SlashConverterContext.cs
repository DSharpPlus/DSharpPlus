namespace DSharpPlus.CommandAll.Processors.SlashCommands;
using System.Collections.Generic;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;

public record SlashConverterContext : ConverterContext
{
    public required DiscordInteraction Interaction { get; init; }
    public required IReadOnlyList<DiscordInteractionDataOption> Options { get; init; }
    public override DiscordInteractionDataOption Argument => this.Options[this.ParameterIndex];
    public int ArgumentIndex { get; private set; } = -1;

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
