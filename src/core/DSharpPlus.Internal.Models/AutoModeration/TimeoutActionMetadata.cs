// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="ITimeoutActionMetadata" />
public sealed record TimeoutActionMetadata : ITimeoutActionMetadata
{
    /// <inheritdoc/>
    public required int DurationSeconds { get; init; }
}