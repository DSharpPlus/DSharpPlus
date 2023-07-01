// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partially populated channel overwrite.
/// </summary>
public interface IPartialChannelOverwrite
{
    /// <summary>
    /// The snowflake identifier of the role or user this overwrite targtes.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// Specifies what kind of entity this overwrite targets.
    /// </summary>
    public Optional<DiscordChannelOverwriteType> Type { get; }

    /// <summary>
    /// The permissions explicitly granted by this overwrite.
    /// </summary>
    public Optional<DiscordPermissions> Allow { get; }

    /// <summary>
    /// The permissions explicitly denied by this overwrite.
    /// </summary>
    public Optional<DiscordPermissions> Deny { get; }
}
