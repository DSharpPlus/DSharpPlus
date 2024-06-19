// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates different display styles for buttons.
/// </summary>
public enum DiscordButtonStyle
{
    /// <summary>
    /// A discord-blurple button.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// A gray button.
    /// </summary>
    Secondary,

    /// <summary>
    /// A green button.
    /// </summary>
    Success,

    /// <summary>
    /// A red button.
    /// </summary>
    Danger,

    /// <summary>
    /// A gray button, navigating to an URL.
    /// </summary>
    Link,

    /// <summary>
    /// A discord-blurple button directing an user to purchase an SKU. 
    /// </summary>
    Premium
}
