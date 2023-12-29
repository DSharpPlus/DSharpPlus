// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Queries;

public readonly record struct ListThreadMembersQuery
{
    /// <summary>
    /// Specifies whether the returned thread member object should contain guild member data.
    /// </summary>
    public bool? WithMember { get; init; }

    /// <summary>
    /// If specified, only request thread members with an ID greater than this ID.
    /// </summary>
    public Snowflake? After { get; init; }

    /// <summary>
    /// The maximum number of entities for this request.
    /// </summary>
    public int? Limit { get; init; }
}