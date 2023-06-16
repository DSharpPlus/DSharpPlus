// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a fully populated emoji object.
/// </summary>
public interface IEmoji : IPartialEmoji
{
    /// <inheritdoc cref="IPartialEmoji.Id"/>
    public new Snowflake? Id { get; }

    /// <inheritdoc cref="IPartialEmoji.Name"/>
    public new string? Name { get; }

    // direct partial access routes

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialEmoji.Id => this.Id;

    /// <inheritdoc/>
    Optional<string?> IPartialEmoji.Name => this.Name;
}
