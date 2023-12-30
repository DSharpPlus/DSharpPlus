// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a permission overwrite on a channel.
/// </summary>
public interface IChannelOverwrite : IPartialChannelOverwrite
{
    /// <inheritdoc cref="IPartialChannelOverwrite.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialChannelOverwrite.Type"/>
    public new DiscordChannelOverwriteType Type { get; }

    /// <inheritdoc cref="IPartialChannelOverwrite.Allow"/>
    public new DiscordPermissions Allow { get; }

    /// <inheritdoc cref="IPartialChannelOverwrite.Deny"/>
    public new DiscordPermissions Deny { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialChannelOverwrite.Id => this.Id;

    /// <inheritdoc/>
    Optional<DiscordChannelOverwriteType> IPartialChannelOverwrite.Type => this.Type;

    /// <inheritdoc/>
    Optional<DiscordPermissions> IPartialChannelOverwrite.Allow => this.Allow;

    /// <inheritdoc/>
    Optional<DiscordPermissions> IPartialChannelOverwrite.Deny => this.Deny;
}
