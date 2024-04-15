namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

using System;

using DSharpPlus.Entities;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class SlashChannelTypesAttribute(params DiscordChannelType[] channelTypes) : Attribute
{
    public DiscordChannelType[] ChannelTypes { get; init; } = channelTypes;
}
