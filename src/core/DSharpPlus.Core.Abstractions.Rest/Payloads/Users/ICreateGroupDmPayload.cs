// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /users/@me/channels</c>.
/// </summary>
public interface ICreateGroupDmPayload
{
    /// <summary>
    /// The access tokens of users that have granted your app the <c>gdm.join</c> scope.
    /// </summary>
    public IReadOnlyList<string> AccessTokens { get; }

    /// <summary>
    /// The nicknames to initialize the users with.
    /// </summary>
    public IReadOnlyDictionary<Snowflake, string> Nicks { get; }
}
