using System;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.ArgumentModifiers;

/// <summary>
/// Specifies what channel types the parameter supports.
/// </summary>
/// <param name="channelTypes">The required types of channels.</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class ChannelTypesAttribute(params DiscordChannelType[] channelTypes) : ParameterCheckAttribute
{
    /// <summary>
    /// Gets the channel types allowed for this parameter.
    /// </summary>
    public DiscordChannelType[] ChannelTypes { get; init; } = channelTypes;
}
