using System;

namespace DSharpPlus.CommandAll.EventProcessors.SlashCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
    public sealed class SlashConverterAttribute : Attribute
    {
        public ApplicationCommandOptionType ParameterType { get; init; }

        public SlashConverterAttribute(ApplicationCommandOptionType parameterType) => ParameterType = parameterType;
    }
}
