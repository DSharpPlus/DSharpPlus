// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates the different types of message components.
/// </summary>
public enum DiscordMessageComponentType
{
    /// <summary>
    /// A container for other components: up to five button or one non-button component.
    /// </summary>
    ActionRow = 1,

    /// <summary>
    /// A clickable button component.
    /// </summary>
    Button,

    /// <summary>
    /// A select menu for picking from application-defined text options.
    /// </summary>
    StringSelect,

    /// <summary>
    /// A text input field.
    /// </summary>
    TextInput,

    /// <summary>
    /// A select menu for picking from users.
    /// </summary>
    UserSelect,

    /// <summary>
    /// A select menu for picking from roles.
    /// </summary>
    RoleSelect,

    /// <summary>
    /// A select menu for picking from mentionable entities (users and roles).
    /// </summary>
    MentionableSelect,

    /// <summary>
    /// A select menu for picking from channels.
    /// </summary>
    ChannelSelect
}
