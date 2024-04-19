// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IThreadMember" />
public sealed record ThreadMember : IThreadMember
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> UserId { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset JoinTimestamp { get; init; }

    /// <inheritdoc/>
    public required int Flags { get; init; }

    /// <inheritdoc/>
    public Optional<IGuildMember> Member { get; init; }
}
