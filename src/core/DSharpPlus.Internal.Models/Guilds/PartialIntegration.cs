// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialIntegration" />
public sealed record PartialIntegration : IPartialIntegration
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<string> Type { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Enabled { get; init; }

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
    public Optional<IIntegrationAccount> Account { get; init; }

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