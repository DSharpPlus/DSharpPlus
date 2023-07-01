// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Specifies the severity of the explicit content filter in the given guild.
/// </summary>
public enum DiscordExplicitContentFilterLevel
{
    /// <summary>
    /// Media content will not be scanned.
    /// </summary>
    Disabled,

    /// <summary>
    /// Media content sent by members without roles will be scanned.
    /// </summary>
    MembersWithoutRoles,

    /// <summary>
    /// Media content sent by all members will be scanned.
    /// </summary>
    AllMembers
}
