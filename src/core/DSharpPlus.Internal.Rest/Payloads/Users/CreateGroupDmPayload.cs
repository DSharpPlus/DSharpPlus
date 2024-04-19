// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateGroupDmPayload" />
public sealed record CreateGroupDmPayload : ICreateGroupDmPayload
{
    /// <inheritdoc/>
    public required IReadOnlyList<string> AccessTokens { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyDictionary<Snowflake, string> Nicks { get; init; }
}
