using System;

namespace DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class SlashChannelTypesAttribute(params ChannelType[] channelTypes) : Attribute
{
    public ChannelType[] ChannelTypes { get; init; } = channelTypes;
}
