// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

public readonly record struct BanMemberQuery
{
    /// <summary>
    /// Specifies how many seconds worth of message history should be deleted along with this ban.
    /// This allows up to 7 days, or 604800 seconds.
    /// </summary>
    public int? DeleteMessageSeconds { get; init; }
}