// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using OneOf;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IAuditLogEntry" />
public sealed record AuditLogEntry : IAuditLogEntry
{
    /// <inheritdoc/>
    public OneOf<string, Snowflake>? TargetId { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IAuditLogChange>> Changes { get; init; }

    /// <inheritdoc/>
    public Snowflake? UserId { get; init; }

    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required DiscordAuditLogEvent ActionType { get; init; }

    /// <inheritdoc/>
    public Optional<IAuditLogEntryInfo> Options { get; init; }

    /// <inheritdoc/>
    public Optional<string> Reason { get; init; }
}