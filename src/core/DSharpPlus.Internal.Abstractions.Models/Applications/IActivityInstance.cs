// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a live instance of a running activity.
/// </summary>
public interface IActivityInstance
{
    /// <summary>
    /// The snowflake identifier of the application executing the activity.
    /// </summary>
    public Snowflake ApplicationId { get; }

    /// <summary>
    /// An unique identifier of the current activity instance.
    /// </summary>
    public string InstanceId { get; }

    /// <summary>
    /// A snowflake identifier created for the launch of this activity.
    /// </summary>
    public Snowflake LaunchId { get; }

    /// <summary>
    /// The guild and channel this activity is running in.
    /// </summary>
    public IActivityLocation Location { get; }

    /// <summary>
    /// The snowflake identifiers of users currently connected to this instance.
    /// </summary>
    public IReadOnlyList<Snowflake> Users { get; }
}
