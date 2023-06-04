// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a permission overwrite on a channel.
/// </summary>
public interface IChannelOverwrite
{
    /// <summary>
    /// The snowflake identifier of the role or user this overwrite targtes.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// Specifies what kind of entity this overwrite targets.
    /// </summary>
    public DiscordChannelOverwriteType Type { get; }

    /// <summary>
    /// The permissions explicitly granted by this overwrite.
    /// </summary>
    public DiscordPermissions Allow { get; }

    /// <summary>
    /// The permissions explicitly denied by this overwrite.
    /// </summary>
    public DiscordPermissions Deny { get; }
}
