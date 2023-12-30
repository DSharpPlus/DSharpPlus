// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Serves as a container object for users and the scheduled event they have subscribed to.
/// </summary>
public interface IScheduledEventUser
{
    /// <summary>
    /// The snowflake identifier of the event this user has subscribed to.
    /// </summary>
    public Snowflake GuildScheduledEventId { get; }
    
    /// <summary>
    /// The user which subscribed to the event.
    /// </summary>
    public IUser User { get; }

    /// <summary>
    /// Guild member data for this user for the guild which this event belongs to.
    /// </summary>
    public Optional<IGuildMember> Member { get; }
}
