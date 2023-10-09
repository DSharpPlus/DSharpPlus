// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a fully populated channel object.
/// </summary>
public interface IChannel : IPartialChannel
{
    /// <inheritdoc cref="IPartialChannel.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialChannel.Type"/>
    public new DiscordChannelType Type { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialChannel.Id => this.Id;

    /// <inheritdoc/>
    Optional<DiscordChannelType> IPartialChannel.Type => this.Type;
}
