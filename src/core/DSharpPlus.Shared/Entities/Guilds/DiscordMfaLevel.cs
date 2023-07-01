// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Indicates multi-factor-auth (MFA) requirements for moderation actions.
/// </summary>
public enum DiscordMfaLevel
{
    /// <summary>
    /// This guild has no MFA requirement for moderation actions.
    /// </summary>
    None,

    /// <summary>
    /// This guild has a 2FA requirement for moderation actions.
    /// </summary>
    Elevated
}
