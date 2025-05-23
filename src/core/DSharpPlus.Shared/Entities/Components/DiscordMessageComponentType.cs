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
    ChannelSelect,

    /// <summary>
    /// A container to display text alongside an accessory component (button or thumbnail).
    /// </summary>
    Section,

    /// <summary>
    /// A component containing markdown text.
    /// </summary>
    TextDisplay,

    /// <summary>
    /// A small image that can be used as an accessory for a <see cref="Section"/>.
    /// </summary>
    Thumbnail,

    /// <summary>
    /// A component displaying media items.
    /// </summary>
    MediaGallery,

    /// <summary>
    /// A component displaying an attached file.
    /// </summary>
    File,

    /// <summary>
    /// A component to add vertical padding and/or display a vertical line between other components.
    /// </summary>
    Separator,

    /// <summary>
    /// A container that visually groups components, akin to an embed.
    /// </summary>
    Container = 17
}
