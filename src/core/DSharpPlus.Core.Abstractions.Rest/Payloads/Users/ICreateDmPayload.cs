// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /users/@me/channels</c>.
/// </summary>
public interface ICreateDmPayload
{
    /// <summary>
    /// The snowflake identifier of the recipient of your DM.
    /// </summary>
    public Snowflake RecipientId { get; }
}
