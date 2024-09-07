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
    StartedOnboarding = 1 << 3,

    /// <summary>
    /// Indicates that this member is a guest and can only access the voice channel they were invited to.
    /// </summary>
    IsGuest = 1 << 4,

    /// <summary>
    /// Indicates that this member has started the 'new member' actions in the server guide.
    /// </summary>
    StartedServerGuide = 1 << 5,

    /// <summary>
    /// Indicates that this member has completed the 'new member' actions in the server guide.
    /// </summary>
    CompletedServerGuide = 1 << 6,

    /// <summary>
    /// Indicates that this member was quarantined by automod for their username, global name or nickname.
    /// </summary>
    AutomodQuarantinedUsername = 1 << 7,

    /// <summary>
    /// Indicates that this member has acknowledged and dismissed the DM settings upsell.
    /// </summary>
    DmSettingsUpsellAcknowledged = 1 << 9
}
