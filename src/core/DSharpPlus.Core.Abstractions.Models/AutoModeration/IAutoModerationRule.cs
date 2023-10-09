// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an auto moderation rule within a guild.
/// </summary>
public interface IAutoModerationRule : IPartialAutoModerationRule
{
    /// <inheritdoc cref="IPartialAutoModerationRule.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.GuildId"/>
    public new Snowflake GuildId { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.CreatorId"/>
    public new Snowflake CreatorId { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.EventType"/>
    public new DiscordAutoModerationEventType EventType { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.TriggerType"/>
    public new DiscordAutoModerationTriggerType TriggerType { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.TriggerMetadata"/>
    public new IAutoModerationTriggerMetadata TriggerMetadata { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.Actions"/>
    public new IReadOnlyList<IAutoModerationAction> Actions { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.Enabled"/>
    public new bool Enabled { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.ExemptRoles"/>
    public new IReadOnlyList<Snowflake> ExemptRoles { get; }

    /// <inheritdoc cref="IPartialAutoModerationRule.ExemptChannels"/>
    public new IReadOnlyList<Snowflake> ExemptChannels { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialAutoModerationRule.Id => this.Id;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialAutoModerationRule.GuildId => this.GuildId;

    /// <inheritdoc/>
    Optional<string> IPartialAutoModerationRule.Name => this.Name;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialAutoModerationRule.CreatorId => this.CreatorId;

    /// <inheritdoc/>
    Optional<DiscordAutoModerationEventType> IPartialAutoModerationRule.EventType => this.EventType;

    /// <inheritdoc/>
    Optional<DiscordAutoModerationTriggerType> IPartialAutoModerationRule.TriggerType => this.TriggerType;

    /// <inheritdoc/>
    Optional<IAutoModerationTriggerMetadata> IPartialAutoModerationRule.TriggerMetadata => new(this.TriggerMetadata);

    /// <inheritdoc/>
    Optional<IReadOnlyList<IAutoModerationAction>> IPartialAutoModerationRule.Actions => new(this.Actions);

    /// <inheritdoc/>
    Optional<bool> IPartialAutoModerationRule.Enabled => this.Enabled;

    /// <inheritdoc/>
    Optional<IReadOnlyList<Snowflake>> IPartialAutoModerationRule.ExemptRoles => new(this.ExemptRoles);

    /// <inheritdoc/>
    Optional<IReadOnlyList<Snowflake>> IPartialAutoModerationRule.ExemptChannels => new(this.ExemptChannels);
}
