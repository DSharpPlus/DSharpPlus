// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// The default notification settings for a guild.
/// </summary>
public enum DiscordMessageNotificationLevel
{
    /// <summary>
    /// Members will, by default, receive notifications for all messages.
    /// </summary>
    AllMessages,

    /// <summary>
    /// Members will, by default, receive notifications only for messages mentioning them specifically.
    /// </summary>
    OnlyMentions
}
