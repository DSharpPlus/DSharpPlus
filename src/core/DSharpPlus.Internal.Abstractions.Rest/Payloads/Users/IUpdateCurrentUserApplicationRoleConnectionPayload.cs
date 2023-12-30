// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PUT /users/@me/applications/:application-id/role-connection</c>.
/// </summary>
public interface IUpdateCurrentUserApplicationRoleConnectionPayload
{
    /// <summary>
    /// The vanity name of the platform the bot is connecting.
    /// </summary>
    public Optional<string> PlatformName { get; }

    /// <summary>
    /// The username on the platform the bot is connecting.
    /// </summary>
    public Optional<string> PlatformUsername { get; }

    /// <summary>
    /// An object mapping application role connection metadata keys to their stringified value.
    /// </summary>
    public Optional<object> Metadata { get; }
}
