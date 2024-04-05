// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/:guild-id/bulk-ban</c>.
/// </summary>
public interface IBulkGuildBanPayload
{
    /// <summary>
    /// The snowflake identifiers of users to bulk ban.
    /// </summary>
    public IReadOnlyList<Snowflake> UserIds { get; }

    /// <summary>
    /// If any of the users have sent messages in the specified amount of seconds, these messages will be deleted if the
    /// ban succeeds.
    /// </summary>
    public Optional<int> DeleteMessageSeconds { get; }
}
