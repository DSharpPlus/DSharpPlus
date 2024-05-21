// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/:guild-id/auto-moderation/rules</c>.
/// </summary>
public interface ICreateAutoModerationRulePayload
{
    /// <summary>
    /// The name of this rule.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of event to trigger evaluation of this rule.
    /// </summary>
    public DiscordAutoModerationEventType EventType { get; }

    /// <summary>
    /// The type of trigger for this rule.
    /// </summary>
    public DiscordAutoModerationTriggerType TriggerType { get; }

    /// <summary>
    /// Additional trigger metadata for this rule.
    /// </summary>
    public Optional<IAutoModerationTriggerMetadata> TriggerMetadata { get; }

    /// <summary>
    /// The actions to execute when this rule is triggered.
    /// </summary>
    public IReadOnlyList<IAutoModerationAction> Actions { get; }

    /// <summary>
    /// Indicates whether the rule is enabled. Defaults to false.
    /// </summary>
    public Optional<bool> Enabled { get; }

    /// <summary>
    /// Up to 20 snowflake identifiers of roles to exempt from this rule.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> ExemptRoles { get; }

    /// <summary>
    /// Up to 50 snowflake identifiers of channels to exempt from this rule.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> ExemptChannels { get; }
}
