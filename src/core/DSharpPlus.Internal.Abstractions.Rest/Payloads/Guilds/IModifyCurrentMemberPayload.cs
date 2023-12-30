// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/members/@me</c>.
/// </summary>
public interface IModifyCurrentMemberPayload
{
    /// <summary>
    /// The nickname of the current user.
    /// </summary>
    public Optional<string?> Nick { get; }
}
