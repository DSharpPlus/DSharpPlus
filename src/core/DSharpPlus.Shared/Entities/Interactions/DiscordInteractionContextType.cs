// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Specifies contexts where an interaction can be used or was triggered from.
/// </summary>
public enum DiscordInteractionContextType
{
    /// <summary>
    /// This interaction can be used or was used within a guild.
    /// </summary>
    Guild,

    /// <summary>
    /// This interaction can be used or was used in a direct message with the bot.
    /// </summary>
    BotDm,

    /// <summary>
    /// This interaction can be used or was used in a DM or group DM without the bot.
    /// </summary>
    PrivateChannel
}
