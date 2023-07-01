// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates the valid types of an application command option.
/// </summary>
public enum DiscordApplicationCommandOptionType
{
    /// <summary>
    /// This option signifies a subcommand of the parent command.
    /// </summary>
    SubCommand = 1,

    /// <summary>
    /// This option signifies a group of subcommands of the parent command.
    /// </summary>
    SubCommandGroup,

    String,

    /// <summary>
    /// Any integer between -2^53 and +2^53.
    /// </summary>
    Integer,

    Boolean,

    User,

    /// <summary>
    /// By default, this includes all channel types + categories.
    /// </summary>
    Channel,

    Role,

    /// <summary>
    /// This includes roles and users.
    /// </summary>
    Mentionable,

    /// <summary>
    /// Any double-precision floating-point number between -2^53 and +2^53.
    /// </summary>
    Number,

    Attachment
}
