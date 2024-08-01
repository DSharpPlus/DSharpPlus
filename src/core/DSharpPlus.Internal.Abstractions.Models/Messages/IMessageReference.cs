// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a message reference, used for replies, crossposts, pins, thread created and thread
/// starter messages, and channel following messages.
/// </summary>
public interface IMessageReference
{
    /// <summary>
    /// The type of this reference. If this is unset, <see cref="DiscordMessageReferenceType.Default"/> should be assumed.
    /// </summary>
    public Optional<DiscordMessageReferenceType> Type { get; }

    /// <summary>
    /// The snowflake identifier of the originating message.
    /// </summary>
    public Optional<Snowflake> MessageId { get; }

    /// <summary>
    /// The snowflake identifier of the originating message's parent channel.
    /// </summary>
    public Optional<Snowflake> ChannelId { get; }

    /// <summary>
    /// The snowflake identifier of the originating message's parent guild.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// Indicates whether this reference will be checked for validity when sending the message; and
    /// whether failing this check should result in this message not sending.
    /// </summary>
    public Optional<bool> FailIfNotExists { get; }
}
