// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates different actions to take when a message is filtered by the automod.
/// </summary>
public enum DiscordAutoModerationActionType
{
    /// <summary>
    /// Prevents the message from being sent. An explanation as to why it was blocked may be specified
    /// and shown to members whenever their message is blocked.
    /// </summary>
    BlockMessage = 1,

    /// <summary>
    /// Logs the message to a specified channel.
    /// </summary>
    SendAlertMessage,

    /// <summary>
    /// Causes an user to be timed out for a specified duration. This can only be applied to rules of types
    /// <see cref="DiscordAutoModerationTriggerType.Keyword"/> and 
    /// <see cref="DiscordAutoModerationTriggerType.MentionSpam"/>.
    /// </summary>
    Timeout
}
