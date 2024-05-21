// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Specifies the type of an invite.
/// </summary>
public enum DiscordInviteType
{
    /// <summary>
    /// An invite to a guild.
    /// </summary>
    Guild,

    /// <summary>
    /// An invite to a group DM.
    /// </summary>
    GroupDm,

    /// <summary>
    /// A friend invite.
    /// </summary>
    Friend
}
