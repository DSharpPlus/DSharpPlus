// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IActivityInstance" />
public sealed record ActivityInstance : IActivityInstance
{
    /// <inheritdoc/>
    public required Snowflake ApplicationId { get; init; }

    /// <inheritdoc/>
    public required string InstanceId { get; init; }

    /// <inheritdoc/>
    public required Snowflake LaunchId { get; init; }

    /// <inheritdoc/>
    public required IActivityLocation Location { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> Users { get; init; }
}