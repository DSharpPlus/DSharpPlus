using System.Collections.Generic;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandAll.Processors.SlashCommands
{
    public record SlashConverterContext : ConverterContext
    {
        public required DiscordInteraction Interaction { get; init; }
        public required IReadOnlyList<DiscordInteractionDataOption> Options { get; init; }
        public override DiscordInteractionDataOption Argument => Options[ParameterIndex];
        public int ArgumentIndex { get; private set; } = -1;

        public override bool NextArgument()
        {
            if (ArgumentIndex + 1 >= Options.Count)
            {
                return false;
            }

            ArgumentIndex++;
            return true;
        }
    }
}
