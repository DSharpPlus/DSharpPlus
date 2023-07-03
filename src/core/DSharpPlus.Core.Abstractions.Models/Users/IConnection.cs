// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an account connection on an user.
/// </summary>
public interface IConnection
{
    /// <summary>
    /// The identifier of the connection account.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The name of the connection acount.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The service for this connection.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Indicates whether this connection has been revoked.
    /// </summary>
    public Optional<bool> Revoked { get; }

    /// <summary>
    /// Corresponding partial integrations.
    /// </summary>
    public Optional<IReadOnlyList<IPartialIntegration>> Integrations { get; }

    /// <summary>
    /// Indicates whether this connection is verified.
    /// </summary>
    public bool Verified { get; }

    /// <summary>
    /// Indicates whether friend sync is enabled for this connection.
    /// </summary>
    public bool FriendSync { get; }

    /// <summary>
    /// Indicates whether activities related to this connection will be shown in presences.
    /// </summary>
    public bool ShowActivity { get; }

    /// <summary>
    /// Indicates whether this connection has a corresponding third-party OAuth2 token.
    /// </summary>
    public bool TwoWayLink { get; }

    /// <summary>
    /// The visibility of this connection.
    /// </summary>
    public DiscordConnectionVisibility Visibility { get; }
}
