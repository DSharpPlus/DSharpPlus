// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IAllowedMentions" />
public sealed record AllowedMentions : IAllowedMentions
{
    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> Parse { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> Roles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> Users { get; init; }

    /// <inheritdoc/>
    public Optional<bool> RepliedUser { get; init; }
}