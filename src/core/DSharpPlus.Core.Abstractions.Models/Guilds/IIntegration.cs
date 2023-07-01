// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an integration between an external service and a guild.
/// </summary>
public interface IIntegration
{
    /// <summary>
    /// The snowflake identifier of the integration.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The name of this integration.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of this integration; twitch, youtube, discord or guild_subscription.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Indicates whether this integration is enabled.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// Indicates whether this integration is synchronizing.
    /// </summary>
    public Optional<bool> Syncing { get; }

    /// <summary>
    /// The snowflake identifier of the role that this integration uses for "subscribers".
    /// </summary>
    public Optional<Snowflake> RoleId { get; }

    /// <summary>
    /// Indicates whether emoticons should be synced for this integration. This is currently only
    /// applicable to twitch.
    /// </summary>
    public Optional<bool> EnableEmoticons { get; }

    /// <summary>
    /// Indicates how this integration should behave when a subscription expires.
    /// </summary>
    public Optional<DiscordIntegrationExpirationBehaviour> ExpireBehaviour { get; }

    /// <summary>
    /// The grace period, in days, before expiring subscribers.
    /// </summary>
    public Optional<int> ExpireGracePeriod { get; }

    /// <summary>
    /// The user for this integration.
    /// </summary>
    public Optional<IUser> User { get; }

    /// <summary>
    /// Contains additional integration account metadata.
    /// </summary>
    public IIntegrationAccount Account { get; }

    /// <summary>
    /// Indicates when this integration was last synced.
    /// </summary>
    public Optional<DateTimeOffset> SyncedAt { get; }

    /// <summary>
    /// The amount of subscribers this integration has.
    /// </summary>
    public Optional<int> SubscriberCount { get; }

    /// <summary>
    /// Indicates whether this integration has been revoked.
    /// </summary>
    public Optional<bool> Revoked { get; }

    /// <summary>
    /// The bot/oauth2 application for discord integrations.
    /// </summary>
    public Optional<IIntegrationApplication> Application { get; }

    /// <summary>
    /// The OAuth2 scopes this application has been authorized for.
    /// </summary>
    public Optional<IReadOnlyList<string>> Scopes { get; }
}
