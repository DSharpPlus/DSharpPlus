namespace DSharpPlus.Commands.Processors.SlashCommands.Attributes;

using System;

using DSharpPlus.Entities;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class SlashChannelTypesAttribute(params DiscordChannelType[] channelTypes) : Attribute
{
    public DiscordChannelType[] ChannelTypes { get; init; } = channelTypes;
}
