// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates additional flags applied to messages.
/// </summary>
[Flags]
public enum DiscordMessageFlags
{
    None = 0,

    /// <summary>
    /// This message has been published to following channels.
    /// </summary>
    Crossposted = 1 << 0,

    /// <summary>
    /// This message originated from a message in another channel this channel is following.
    /// </summary>
    IsCrosspost = 1 << 1,

    /// <summary>
    /// Indicates whether embeds on this message are to be displayed or not.
    /// </summary>
    SuppressEmbeds = 1 << 2,

    /// <summary>
    /// Indicates that this is a crossposted message whose source has been deleted.
    /// </summary>
    SourceMessageDeleted = 1 << 3,

    /// <summary>
    /// Indicates that this is a message originating from the urgent messaging system.
    /// </summary>
    Urgent = 1 << 4,

    /// <summary>
    /// Indicates that this message has an associated thread with the same identifier as this message.
    /// </summary>
    HasThread = 1 << 5,

    /// <summary>
    /// Indicates that this message is only visible to the user who invoked the interaction.
    /// </summary>
    Ephemeral = 1 << 6,

    /// <summary>
    /// Indicates that this message is an interaction response and that the bot is 'thinking'.
    /// </summary>
    Loading = 1 << 7,

    /// <summary>
    /// This message failed to mention some roles and add their members to the thread.
    /// </summary>
    FailedToEnforceSomeRolesInThread = 1 << 8,

    /// <summary>
    /// This message will not trigger push and desktop notifications.
    /// </summary>
    SuppressNotifications = 1 << 12,

    /// <summary>
    /// This message is a voice message.
    /// </summary>
    IsVoiceMessage = 1 << 13,

    /// <summary>
    /// This message contains a message snapshot, via forwarding.
    /// </summary>
    HasSnapshot = 1 << 14,

    /// <summary>
    /// This message contains layout components and does not contain content, embeds, polls or stickers.
    /// </summary>
    EnableLayoutComponents = 1 << 15,
}
