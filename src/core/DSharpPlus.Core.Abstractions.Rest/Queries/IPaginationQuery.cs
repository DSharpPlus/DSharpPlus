// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Queries;

/// <summary>
/// Represents a base interface for all snowflake-paginated queries, for user convenience.
/// </summary>
// this is here despite not being technically necessary to accurately implement queries because
// as of now, it can't go anywhere else. we'll move this once possible, using explicit extensions
// acting as interface adapters.
public interface IPaginationQuery
{
    /// <summary>
    /// If specified, only request entities with an ID less than this ID.
    /// </summary>
    public Snowflake? Before { get; init; }

    /// <summary>
    /// If specified, only request entities with an ID greater than this ID.
    /// </summary>
    public Snowflake? After { get; init; }

    /// <summary>
    /// The maximum number of entities for this request.
    /// </summary>
    public int? Limit { get; init; }
}