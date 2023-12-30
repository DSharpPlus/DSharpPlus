// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Includes additional metadata about a member's presence inside a thread.
/// </summary>
public interface IThreadMember
{
    /// <summary>
    /// The snowflake identifier of the thread this object belongs to.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The snowflake identifier of the user this object belongs to.
    /// </summary>
    public Optional<Snowflake> UserId { get; }

    /// <summary>
    /// The timestamp at which this user last joined the thread.
    /// </summary>
    public DateTimeOffset JoinTimestamp { get; }

    /// <summary>
    /// User thread settings used for notifications.
    /// </summary>
    public int Flags { get; }

    /// <summary>
    /// Additional information about this thread member.
    /// </summary>
    public Optional<IGuildMember> Member { get; }
}
