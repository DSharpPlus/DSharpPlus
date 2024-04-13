namespace DSharpPlus.Commands.Processors.SlashCommands.Attributes;

using System;

using DSharpPlus.Entities;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SlashCommandTypesAttribute(params DiscordApplicationCommandType[] applicationCommandTypes) : Attribute
{
    public DiscordApplicationCommandType[] ApplicationCommandTypes { get; init; } = applicationCommandTypes;
}
