// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents additional settings for system channels.
/// </summary>
[Flags]
public enum DiscordSystemChannelFlags
{
    None = 0,

    /// <summary>
    /// Disables member join messages in this channel.
    /// </summary>
    SuppressJoinNotifications = 1 << 0,

    /// <summary>
    /// Disables server boost notifications in this channel.
    /// </summary>
    SuppressPremiumSubscriptions = 1 << 1,

    /// <summary>
    /// Disables server setup tips in this channnel.
    /// </summary>
    SuppressGuildReminderNotifications = 1 << 2,

    /// <summary>
    /// Disables the sticker reply buttons on member join messages.
    /// </summary>
    SuppressJoinNotificationReplies = 1 << 3,

    /// <summary>
    /// Disables role subscription purchase and renewal notifications in this channel.
    /// </summary>
    SuppressRoleSubscriptionPurchaseNotifications = 1 << 4,

    /// <summary>
    /// Disables the sticker reply buttons on role subscription purchase and renewal messages.
    /// </summary>
    SuppressRoleSubscriptionPurchaseNotificationReplies = 1 << 5
}
