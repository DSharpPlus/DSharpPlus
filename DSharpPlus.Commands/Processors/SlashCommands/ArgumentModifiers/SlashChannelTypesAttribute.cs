using System;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class SlashChannelTypesAttribute(params DiscordChannelType[] channelTypes) : Attribute
{
    public DiscordChannelType[] ChannelTypes { get; init; } = channelTypes;
}
