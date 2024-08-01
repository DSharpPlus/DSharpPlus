// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a mention of a guild channel.
/// </summary>
public interface IChannelMention
{
    /// <summary>
    /// The snowflake identifier of this channel.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The snowflake identifier of the guild containing this channel.
    /// </summary>
    public Snowflake GuildId { get; }

    /// <summary>
    /// The type of this channel.
    /// </summary>
    public DiscordChannelType Type { get; }

    /// <summary>
    /// The name of this channel.
    /// </summary>
    public string Name { get; }
}
