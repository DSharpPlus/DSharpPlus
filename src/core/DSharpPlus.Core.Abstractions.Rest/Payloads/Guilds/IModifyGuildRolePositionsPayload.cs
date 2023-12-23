// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/roles</c>.
/// </summary>
public interface IModifyGuildRolePositionsPayload
{
    /// <summary>
    /// The snowflake identifier of the role to move.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The new sorting position of the role.
    /// </summary>
    public Optional<int?> Position { get; }
}
