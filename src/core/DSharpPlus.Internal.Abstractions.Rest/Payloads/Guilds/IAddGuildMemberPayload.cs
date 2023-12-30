// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PUT /guilds/:guild-id/members/:user-id</c>.
/// </summary>
public interface IAddGuildMemberPayload
{
    /// <summary>
	/// An OAuth2 access token granted with the <c>guilds.join</c> scope.
	/// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// The nickname to initialize the user with.
    /// </summary>
    public Optional<string> Nickname { get; }

    /// <summary>
    /// An array of role IDs to assign immediately upon join.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> Roles { get; }

    /// <summary>
    /// Whether to immediately mute the user upon join.
    /// </summary>
    public Optional<bool> Mute { get; }

    /// <summary>
    /// Whether to immediately deafen the user upon join.
    /// </summary>
    public Optional<bool> Deaf { get; }
}
