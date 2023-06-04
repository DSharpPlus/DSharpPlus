// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Channel flags applied to the containing channel as bit fields.
/// </summary>
[Flags]
public enum DiscordChannelFlags
{
    None,

    /// <summary>
    /// Indicates whether this is a thread channel pinned to the top of its parent forum channel.
    /// </summary>
    Pinned = 1 << 1,

    /// <summary>
    /// Indicates whether this is a forum channel which requires tags to be specified when creating
    /// a thread inside.
    /// </summary>
    RequireTag = 1 << 4
}
