namespace DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;

using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SlashCommandTypesAttribute(params ApplicationCommandType[] applicationCommandTypes) : Attribute
{
    public ApplicationCommandType[] ApplicationCommandTypes { get; init; } = applicationCommandTypes;
}
