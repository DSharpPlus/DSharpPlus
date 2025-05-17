using System;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SlashCommandTypesAttribute(params DiscordApplicationCommandType[] applicationCommandTypes) : Attribute
{
    public DiscordApplicationCommandType[] ApplicationCommandTypes { get; init; } = applicationCommandTypes;
}
