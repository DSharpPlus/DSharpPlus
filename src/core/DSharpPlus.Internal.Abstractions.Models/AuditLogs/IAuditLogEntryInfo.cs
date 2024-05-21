// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Contains additional metadata for an audit log entry.
/// </summary>
/// <remarks>
/// Presence of a field is dictated by its parent <see cref="IAuditLogEntry.ActionType"/>. When deciding
/// what to access, refer to the 
/// <see href="https://discord.com/developers/docs/resources/audit-log#audit-log-entry-object-optional-audit-entry-info">
/// Event Types section in the docs</see>.
/// </remarks>
public interface IAuditLogEntryInfo
{
    /// <summary>
    /// The snowflake identifier of the application whose permissions were targeted.
    /// </summary>
    public Optional<Snowflake> ApplicationId { get; }

    /// <summary>
    /// The name of the auto moderation rule that was triggered.
    /// </summary>
    public Optional<string> AutoModerationRuleName { get; }

    /// <summary>
    /// The trigger type of the auto moderation rule that was triggered.
    /// </summary>
    public Optional<string> AutoModerationRuleTriggerType { get; }

    /// <summary>
    /// The snowflake identifier of the channel in which entities were targeted.
    /// </summary>
    public Optional<Snowflake> ChannelId { get; }

    /// <summary>
    /// The amount of entities that were targeted.
    /// </summary>
    public Optional<string> Count { get; }

    /// <summary>
    /// The amount of days after which inactive members were kicked.
    /// </summary>
    public Optional<string> DeleteMemberDays { get; }

    /// <summary>
    /// The snowflake identifier of the overwritten entry.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The amount of members removed by the server prune.
    /// </summary>
    public Optional<string> MembersRemoved { get; }

    /// <summary>
    /// The snowflake identifier of the message that was targeted.
    /// </summary>
    public Optional<Snowflake> MessageId { get; }

    /// <summary>
    /// The name of the targeted role.
    /// </summary>
    public Optional<string> RoleName { get; }

    /// <summary>
    /// The type of the overwritten entity.
    /// </summary>
    public Optional<string> Type { get; }
}
