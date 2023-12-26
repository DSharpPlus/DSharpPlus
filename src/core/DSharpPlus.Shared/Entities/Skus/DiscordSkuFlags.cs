// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents additional flags for a given SKU.
/// </summary>
[Flags]
public enum DiscordSkuFlags
{
    /// <summary>
    /// This SKU is available for purchase.
    /// </summary>
    Available = 1 << 2,

    /// <summary>
    /// A subscription purchased by a user and applied to a single server. Everyone in that server gets access
    /// to the given SKU.
    /// </summary>
    GuildSubscription = 1 << 7,

    /// <summary>
    /// A subscription purchased by a user for themselves. They get access to the given SKU in every server.
    /// </summary>
    UserSubscription = 1 << 8
}
