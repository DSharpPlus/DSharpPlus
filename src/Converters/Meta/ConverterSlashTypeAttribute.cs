using System;

namespace DSharpPlus.CommandAll.Converters.Meta
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
    public sealed class ConverterSlashTypeAttribute : Attribute
    {
        public ApplicationCommandOptionType ParameterType { get; init; }

        public ConverterSlashTypeAttribute(ApplicationCommandOptionType parameterType) => ParameterType = parameterType;
    }
}
