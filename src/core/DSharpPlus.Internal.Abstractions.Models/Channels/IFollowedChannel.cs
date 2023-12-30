// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a followed channel, whose messages will be crossposted.
/// </summary>
public interface IFollowedChannel
{
    /// <summary>
    /// The snowflake identifier of the source channel.
    /// </summary>
    public Snowflake ChannelId { get; }

    /// <summary>
    /// The snowflake identifier of the crossposting webhook.
    /// </summary>
    public Snowflake WebhookId { get; }
}
