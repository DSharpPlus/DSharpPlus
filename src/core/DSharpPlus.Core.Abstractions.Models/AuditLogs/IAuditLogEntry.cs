// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using OneOf;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a single entry within the audit log.
/// </summary>
public interface IAuditLogEntry
{
    /// <summary>
    /// The identifier of the affected entity.
    /// </summary>
    public OneOf<string, Snowflake>? TargetId { get; }

    /// <summary>
    /// The changes made to the affected entity.
    /// </summary>
    public Optional<IReadOnlyList<IAuditLogChange>> Changes { get; }

    /// <summary>
    /// The user or application that made these changes.
    /// </summary>
    public Snowflake? UserId { get; }

    /// <summary>
    /// The snowflake identifier of this entry.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The type of the action that occurred.
    /// </summary>
    public DiscordAuditLogEvent ActionType { get; }

    /// <summary>
    /// Additional information sent for certain event types.
    /// </summary>
    public Optional<IAuditLogEntryInfo> Options { get; }

    /// <summary>
    /// The reason for this change.
    /// </summary>
    public Optional<string> Reason { get; }
}
