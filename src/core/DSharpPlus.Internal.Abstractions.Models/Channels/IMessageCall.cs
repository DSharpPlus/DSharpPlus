// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Contains information about a call in a private channel.
/// </summary>
public interface IMessageCall
{
    /// <summary>
    /// The snowflake IDs of participating users.
    /// </summary>
    public IReadOnlyList<Snowflake> Participants { get; }

    /// <summary>
    /// The timestamp at which the call ended.
    /// </summary>
    public Optional<DateTimeOffset?> EndedTimestamp { get; }
}
