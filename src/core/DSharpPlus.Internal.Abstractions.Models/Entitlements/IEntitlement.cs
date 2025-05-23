// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents an entitlement to a premium offering in an application.
/// </summary>
public interface IEntitlement : IPartialEntitlement
{
    /// <inheritdoc cref="IPartialEntitlement.SkuId"/>
    public new Snowflake SkuId { get; }

    /// <inheritdoc cref="IPartialEntitlement.ApplicationId"/>
    public new Snowflake ApplicationId { get; }

    /// <inheritdoc cref="IPartialEntitlement.Type"/>
    public new DiscordEntitlementType Type { get; }

    /// <inheritdoc cref="IPartialEntitlement.Deleted"/>
    public new bool Deleted { get; }

    /// <inheritdoc cref="IPartialEntitlement.StartsAt"/>
    public new DateTimeOffset? StartsAt { get; }

    /// <inheritdoc cref="IPartialEntitlement.EndsAt"/>
    public new DateTimeOffset? EndsAt { get; }

    // partial access

    Optional<Snowflake> IPartialEntitlement.SkuId => this.SkuId;

    Optional<Snowflake> IPartialEntitlement.ApplicationId => this.ApplicationId;

    Optional<DiscordEntitlementType> IPartialEntitlement.Type => this.Type;

    Optional<bool> IPartialEntitlement.Deleted => this.Deleted;

    Optional<DateTimeOffset?> IPartialEntitlement.StartsAt => this.StartsAt;

    Optional<DateTimeOffset?> IPartialEntitlement.EndsAt => this.EndsAt;
}
