// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates the possible application command types.
/// </summary>
public enum DiscordApplicationCommandType
{
    /// <summary>
    /// Slash commands, text based commands that show up in the chat box.
    /// </summary>
    ChatInput = 1,

    /// <summary>
    /// User context menu based commands.
    /// </summary>
    User,

    /// <summary>
    /// Message context menu based commands.
    /// </summary>
    Message
}
