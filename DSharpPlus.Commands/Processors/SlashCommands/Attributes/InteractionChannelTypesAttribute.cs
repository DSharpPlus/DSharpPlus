namespace DSharpPlus.Commands.Processors.SlashCommands.Attributes;

using System;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class InteractionChannelTypesAttribute(params ChannelType[] channelTypes) : Attribute
{
    public ChannelType[] ChannelTypes { get; init; } = channelTypes;
}
