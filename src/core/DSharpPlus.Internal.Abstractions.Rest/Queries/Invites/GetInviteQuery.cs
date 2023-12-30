// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains query parameters to <c>IInviteRestAPI.GetInviteAsync</c>.
/// </summary>
public readonly record struct GetInviteQuery
{
    /// <summary>
    /// Specifies whether the returned invite object should include approximate member counts.
    /// </summary>
    public bool? WithCounts { get; init; }

    /// <summary>
    /// Specifies whether the returned invite object should include the expiration date.
    /// </summary>
    public bool? WithExpiration { get; init; }

    /// <summary>
    /// The snowflake identifier of the scheduled event to include with the invite.
    /// </summary>
    public Snowflake? GuildScheduledEventId { get; init; }
}