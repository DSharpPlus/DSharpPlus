// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IPartialApplicationCommandPermissions" />
public sealed record PartialApplicationCommandPermissions : IPartialApplicationCommandPermissions
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IApplicationCommandPermission>> Permissions { get; init; }
}