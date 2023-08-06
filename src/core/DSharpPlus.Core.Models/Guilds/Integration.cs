// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IIntegration" />
public sealed record Integration : IIntegration
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required string Type { get; init; }

    /// <inheritdoc/>
    public required bool Enabled { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Syncing { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> RoleId { get; init; }

    /// <inheritdoc/>
    public Optional<bool> EnableEmoticons { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordIntegrationExpirationBehaviour> ExpireBehaviour { get; init; }

    /// <inheritdoc/>
    public Optional<int> ExpireGracePeriod { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public required IIntegrationAccount Account { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> SyncedAt { get; init; }

    /// <inheritdoc/>
    public Optional<int> SubscriberCount { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Revoked { get; init; }

    /// <inheritdoc/>
    public Optional<IIntegrationApplication> Application { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> Scopes { get; init; }
}