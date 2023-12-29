// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Queries;

/// <summary>
/// Contains query parameters for <c>IChannelRestAPI.GetChannelMessagesAsync</c>.
/// </summary>
public readonly record struct GetChannelMessagesQuery : IPaginationQuery
{
    /// <summary>
    /// If specified, request entities around this ID.
    /// </summary>
    /// <remarks>
    /// Mutually exclusive with <seealso cref="Before"/> and <seealso cref="After"/>.
    /// </remarks>
    public Snowflake? Around { get; init; }

    /// <inheritdoc/>
    /// <remarks>
    /// Mutually exclusive with <seealso cref="Around"/> and <seealso cref="After"/>.
    /// </remarks>
    public Snowflake? Before { get; init; }

    /// <inheritdoc/>
    /// <remarks>
    /// Mutually exclusive with <seealso cref="Around"/> and <seealso cref="Before"/>.
    /// </remarks>
    public Snowflake? After { get; init; }

    /// <inheritdoc/>
    public int? Limit { get; init; }
}