// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a partial auto-moderation rule.
/// </summary>
public interface IPartialAutoModerationRule
{
    /// <summary>
    /// The snowflake identifier of this rule.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The snowflake identifier of the guild this rule belongs to.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The display name of this rule.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The snowflake identifier of the user who created this rule.
    /// </summary>
    public Optional<Snowflake> CreatorId { get; }

    /// <summary>
    /// The type of event that causes this rule to fire.
    /// </summary>
    public Optional<DiscordAutoModerationEventType> EventType { get; }

    /// <summary>
    /// The trigger type of this rule.
    /// </summary>
    public Optional<DiscordAutoModerationTriggerType> TriggerType { get; }

    /// <summary>
    /// Additional metadata for this rule trigger.
    /// </summary>
    public Optional<IAutoModerationTriggerMetadata> TriggerMetadata { get; }

    /// <summary>
    /// The actions which wille xecute when this rule is triggered.
    /// </summary>
    public Optional<IReadOnlyList<IAutoModerationAction>> Actions { get; }

    /// <summary>
    /// Indicates whether this rule should be enabled.
    /// </summary>
    public Optional<bool> Enabled { get; }

    /// <summary>
    /// Up to 20 role IDs that should be exempted from this rule.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> ExemptRoles { get; }

    /// <summary>
    /// Up to 50 channel IDs that should be exempted from this rule.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> ExemptChannels { get; }
}
