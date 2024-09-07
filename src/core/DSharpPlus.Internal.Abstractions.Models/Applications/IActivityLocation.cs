// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Specifies the location an activity is running in.
/// </summary>
public interface IActivityLocation
{
    /// <summary>
    /// A unique identifier for this location.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Specifies what this location is: "gc" for a guild channel, "pc" for a private channel such as direct message
    /// or group chat.
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// The snowflake identifier of the channel hosting the activity.
    /// </summary>
    public Snowflake ChannelId { get; }

    /// <summary>
    /// The snowflake identifier of the guild hosting the activity.
    /// </summary>
    public Optional<Snowflake?> GuildId { get; }
}
