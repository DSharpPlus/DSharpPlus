// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates the levels of verification restriction required to speak in a guild.
/// </summary>
public enum DiscordVerificationLevel
{
    /// <summary>
    /// Unrestricted.
    /// </summary>
    None,

    /// <summary>
    /// The user must have a verified email.
    /// </summary>
    Low,

    /// <summary>
    /// This user must be registered on Discord for longer than five minutes.
    /// </summary>
    Medium,

    /// <summary>
    /// This user must have been a member of this server for longer than ten minutes.
    /// </summary>
    High,

    /// <summary>
    /// This user must have a verified phone number.
    /// </summary>
    VeryHigh
}
