using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Defines allowed channel types for a channel parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class ChannelTypesAttribute : Attribute
{
    /// <summary>
    /// Allowed channel types.
    /// </summary>
    public IEnumerable<DiscordChannelType> ChannelTypes { get; }

    /// <summary>
    /// Defines allowed channel types for a channel parameter.
    /// </summary>
    /// <param name="channelTypes">The channel types to allow.</param>
    public ChannelTypesAttribute(params DiscordChannelType[] channelTypes) => this.ChannelTypes = channelTypes;
}
