// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateChannelInvitePayload" />
public sealed record CreateChannelInvitePayload : ICreateChannelInvitePayload
{
    /// <inheritdoc/>
    public Optional<int> MaxAge { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxUses { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Temporary { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Unique { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordInviteTargetType> TargetType { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> TargetUserId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> TargetApplicationId { get; init; }
}