// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PUT /guilds/:guild-id/incident-actions</c>.
/// </summary>
public interface IModifyGuildIncidentActionsPayload
{
    /// <summary>
    /// Disables invites up until the specified time.
    /// </summary>
    public Optional<DateTimeOffset?> InvitesDisabledUntil { get; }

    /// <summary>
    /// Disables direct messages between guild members up until the specified time. Note that this does not prevent members from messaging each other
    /// if they have another avenue of doing so, such as another mutual server or being friends.
    /// </summary>
    public Optional<DateTimeOffset?> DmsDisabledUntil { get; }
}
