// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to PUT /channel/:channel-id/recipients/:user-id.
/// </summary>
public interface IGroupDMAddRecipientPayload
{
    /// <summary>
    /// The access token of the user, which must have granted you the <c>gdm.join</c> oauth scope.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// The nickname of the user, to be given on join.
    /// </summary>
    public string Nick { get; init; }
}
