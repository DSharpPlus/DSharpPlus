// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>GET /applications/:application-id/emojis</c>.
/// </summary>
public readonly record struct ListApplicationEmojisResponse
{
    public IReadOnlyList<IEmoji> Items { get; init; }
}
