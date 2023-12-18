// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload for <c>PUT /channels/:channel-id/permissions/:overwrite-id</c>.
/// </summary>
public interface IEditChannelPermissionsPayload
{
    /// <summary>
    /// The overwrite type - either role or member.
    /// </summary>
    public DiscordChannelOverwriteType Type { get; }

    /// <summary>
    /// The permissions this overwrite should grant.
    /// </summary>
    public Optional<DiscordPermissions?> Allow { get; }

    /// <summary>
    /// The permissions this overwrite should deny.
    /// </summary>
    public Optional<DiscordPermissions?> Deny { get; }
}
