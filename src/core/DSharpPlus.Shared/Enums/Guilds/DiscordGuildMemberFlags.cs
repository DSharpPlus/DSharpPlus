// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents bitwise guild member flags.
/// </summary>
[Flags]
public enum DiscordGuildMemberFlags
{
    None = 0,
    
    /// <summary>
    /// Indicates that this member has left and rejoined this guild.
    /// </summary>
    DidRejoin = 1 << 0,

    /// <summary>
    /// Indicates that this member has completed guild onboarding.
    /// </summary>
    CompletedOnboarding = 1 << 1,

    /// <summary>
    /// Indicates that this member is exempt from verification requirements. This flag can be set
    /// by bots.
    /// </summary>
    BypassesVerification = 1 << 2,

    /// <summary>
    /// Indicates that this member has started the guild onboarding process.
    /// </summary>
    StartedOnboarding = 1 << 3
}
