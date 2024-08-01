// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains query parameters for <c>IChannelRestAPI.GetChannelMessagesAsync</c>.
/// </summary>
public readonly record struct GetChannelMessagesQuery : IPaginatedQuery
{
    /// <summary>
    /// If specified, request entities around this ID.
    /// </summary>
    /// <remarks>
    /// Mutually exclusive with <see cref="Before"/> and <see cref="After"/>.
    /// </remarks>
    public Snowflake? Around { get; init; }

    /// <inheritdoc/>
    /// <remarks>
    /// Mutually exclusive with <see cref="Around"/> and <see cref="After"/>.
    /// </remarks>
    public Snowflake? Before { get; init; }

    /// <inheritdoc/>
    /// <remarks>
    /// Mutually exclusive with <see cref="Around"/> and <see cref="Before"/>.
    /// </remarks>
    public Snowflake? After { get; init; }

    /// <inheritdoc/>
    public int? Limit { get; init; }
}
