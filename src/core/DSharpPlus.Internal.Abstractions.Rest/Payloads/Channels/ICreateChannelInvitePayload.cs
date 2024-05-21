// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /channels/:channel-id/invites</c>.
/// </summary>
public interface ICreateChannelInvitePayload
{
    /// <summary>
    /// Specifies the expiry time in seconds for this invite. Setting it to 0 means the invite never expires.
    /// </summary>
    public Optional<int> MaxAge { get; init; }

    /// <summary>
    /// Specifies the maximum amount of uses for this invite. Setting it to 0 means the invite can be used infinitely.
    /// </summary>
    public Optional<int> MaxUses { get; init; }

    /// <summary>
    /// Indicates whether this invite only grants temporary membership.
    /// </summary>
    public Optional<bool> Temporary { get; init; }

    /// <summary>
    /// Specifies whether this invite is unique. If true, Discord will not try to reuse a similar invite.
    /// </summary>
    public Optional<bool> Unique { get; init; }

    /// <summary>
    /// Specifies the target type of this voice channel invite.
    /// </summary>
    public Optional<DiscordInviteTargetType> TargetType { get; init; }

    /// <summary>
    /// Snowflake identifier of the invite's target user if <see cref="TargetType"/> is
    /// <see cref="DiscordInviteTargetType.Stream"/>.
    /// </summary>
    public Optional<Snowflake> TargetUserId { get; init; }

    /// <summary>
    /// Snowflake identifier of the invite's target embedded application if <see cref="TargetType"/> is
    /// <see cref="DiscordInviteTargetType.EmbeddedApplication"/>.
    /// </summary>
    public Optional<Snowflake> TargetApplicationId { get; init; }
}
