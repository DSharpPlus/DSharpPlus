// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /channels/:channel-id/messages/bulk-delete</c>.
/// </summary>
public interface IBulkDeleteMessagesPayload
{
    /// <summary>
    /// The message IDs to bulk delete, between 2 and 100.
    /// </summary>
    public IReadOnlyList<Snowflake> Messages { get; }
}
