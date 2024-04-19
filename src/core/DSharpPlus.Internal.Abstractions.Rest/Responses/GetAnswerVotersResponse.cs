// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>GET /channels/:channel-id/polls/:message-id/answers/:answer-id</c>.
/// </summary>
public readonly record struct GetAnswerVotersResponse
{
    /// <summary>
    /// The users who voted for this answer.
    /// </summary>
    public IReadOnlyList<IUser> Users { get; init; }
}
