// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an user object.
/// </summary>
public interface IUser : IPartialUser
{
    /// <inheritdoc cref="IPartialUser.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialUser.Username"/>
    public new string Username { get; }

    /// <inheritdoc cref="IPartialUser.Discriminator"/>
    public new string Discriminator { get; }

    /// <inheritdoc cref="IPartialUser.GlobalName"/>
    public new string? GlobalName { get; }

    /// <inheritdoc cref="IPartialUser.Avatar"/>
    public new string? Avatar { get; }

    // explicit routes for partial user access

    /// <inheritdoc/>
    Optional<Snowflake> IPartialUser.Id => this.Id;

    /// <inheritdoc/>
    Optional<string> IPartialUser.Username => this.Username;

    /// <inheritdoc/>
    Optional<string> IPartialUser.Discriminator => this.Discriminator;

    /// <inheritdoc/>
    Optional<string?> IPartialUser.GlobalName => this.GlobalName;

    /// <inheritdoc/>
    Optional<string?> IPartialUser.Avatar => this.Avatar;
}
