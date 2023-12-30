// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters for <c>IChannelRestAPI.GetThreadMemberAsync</c>.
/// </summary>
public readonly record struct GetThreadMemberQuery
{
    /// <summary>
    /// Specifies whether the returned thread member object should contain guild member data.
    /// </summary>
    public bool? WithMember { get; init; }
}