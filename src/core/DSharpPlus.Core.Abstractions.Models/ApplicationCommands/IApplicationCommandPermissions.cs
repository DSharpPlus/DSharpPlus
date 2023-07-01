// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a collection of permissions for an application command in a guild
/// </summary>
public interface IApplicationCommandPermissions : IPartialApplicationCommandPermissions
{
    /// <inheritdoc cref="IPartialApplicationCommandPermissions.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialApplicationCommandPermissions.ApplicationId"/>
    public new Snowflake ApplicationId { get; }

    /// <inheritdoc cref="IPartialApplicationCommandPermissions.GuildId"/>
    public new Snowflake GuildId { get; }

    /// <inheritdoc cref="IPartialApplicationCommandPermissions.Permissions"/>
    public new IReadOnlyList<IApplicationCommandPermission> Permissions { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialApplicationCommandPermissions.Id => this.Id;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialApplicationCommandPermissions.ApplicationId => this.ApplicationId;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialApplicationCommandPermissions.GuildId => this.GuildId;

    /// <inheritdoc/>
    Optional<IReadOnlyList<IApplicationCommandPermission>> IPartialApplicationCommandPermissions.Permissions => new(this.Permissions);
}
