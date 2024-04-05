// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>POST /guilds/:guild-id/bulk-ban</c>.
/// </summary>
public readonly record struct BulkGuildBanResponse
{
    /// <summary>
    /// The snowflakes of users that were successfully banned.
    /// </summary>
    public required IReadOnlyList<Snowflake> BannedUsers { get; init; }

    /// <summary>
    /// The snowflakes of users that could not be banned.
    /// </summary>
    public required IReadOnlyList<Snowflake> FailedUsers { get; init; }
}
