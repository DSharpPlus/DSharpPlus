// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/members/:user-id</c>.
/// </summary>
public interface IModifyGuildMemberPayload
{
    /// <summary>
    /// The nickname to force the user to assume.
    /// </summary>
    public Optional<string?> Nickname { get; }

    /// <summary>
    /// An array of role IDs to assign.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>?> Roles { get; }

    /// <summary>
    /// Whether to mute the user.
    /// </summary>
    public Optional<bool?> Mute { get; }

    /// <summary>
    /// Whether to deafen the user.
    /// </summary>
    public Optional<bool?> Deaf { get; }

    /// <summary>
    /// The voice channel ID to move the user into.
    /// </summary>
    public Optional<Snowflake?> ChannelId { get; }

    /// <summary>
    /// The timestamp at which the user's timeout is supposed to expire. Set to null to remove the timeout.
    /// Must be no more than 28 days in the future.
    /// </summary>
    public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; }
}
