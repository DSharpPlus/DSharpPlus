// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents flags for a role.
/// </summary>
[Flags]
public enum DiscordRoleFlags
{
    /// <summary>
    /// Indicates that this role can be selected by members in an onboarding prompt.
    /// </summary>
    InPrompt = 1 << 0
}
