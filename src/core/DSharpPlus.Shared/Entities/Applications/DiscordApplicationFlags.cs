// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates flags for applications and thereby bots.
/// </summary>
[Flags]
public enum DiscordApplicationFlags
{
    /// <summary>
    /// Indicates whether an application uses the auto moderation API.
    /// </summary>
    ApplicationAutoModerationRuleCreateBadge = 1 << 6,

    /// <summary>
    /// The intent required for bots in 100 or more servers to receive presence update events.
    /// </summary>
    GatewayPresence = 1 << 12,

    /// <summary>
    /// The intent required for bots in less than 100 servers to receive presence update events.
    /// Unlike <seealso cref="GatewayPresence"/>, this does not require staff approval.
    /// </summary>
    GatewayPresenceLimited = 1 << 13,

    /// <summary>
    /// The intent required for bots in 100 or more servers to receive guild member related events.
    /// </summary>
    GatewayGuildMembers = 1 << 14,

    /// <summary>
    /// The intent required for bots in less than 100 servers to receive guild member related events.
    /// Unlike <seealso cref="GatewayGuildMembers"/>, this does not require staff approval.
    /// </summary>
    GatewayGuildMembersLimited = 1 << 15,

    /// <summary>
    /// Indicates unusual growth of an app that prevents verification.
    /// </summary>
    VerificationPendingGuildLimit = 1 << 16,

    /// <summary>
    /// Indicates whether this app is embedded into the Discord client.
    /// </summary>
    Embedded = 1 << 17,

    /// <summary>
    /// The intent required for bots in 100 or more servers to receive message content.
    /// </summary>
    GatewayMessageContent = 1 << 18,

    /// <summary>
    /// The intent required for bots in less than 100 servers to receive message content.
    /// Unlike <seealso cref="GatewayMessageContent"/>, this does not require staff approval.
    /// </summary>
    GatewayMessageContentLimited = 1 << 19,

    /// <summary>
    /// Indicates whether this application has registered global application commands.
    /// </summary>
    ApplicationCommandBadge = 1 << 23
}
