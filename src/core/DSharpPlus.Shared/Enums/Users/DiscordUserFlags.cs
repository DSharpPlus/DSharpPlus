// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates all exposed and documented user flags.
/// </summary>
[Flags]
public enum DiscordUserFlags
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// Profile badge indicating this user is a Discord employee.
    /// </summary>
    DiscordEmployee = 1 << 0,

    /// <summary>
    /// Profile badge indicating this user owns a Partnered server.
    /// </summary>
    PartneredServerOwner = 1 << 1,

    /// <summary>
    /// Profile badge indicating this user has attended a real-world HypeSquad event.
    /// </summary>
    HypeSquadEvents = 1 << 2,

    /// <summary>
    /// First of two badges for bug hunters. Discord doesn't tell us any further information.
    /// </summary>
    BugHunterLevel1 = 1 << 3, 

    /// <summary>
    /// Profile badge indicating this user is a member of the HypeSquad House of Bravery.
    /// </summary>
    HouseBravery = 1 << 6,

    /// <summary>
    /// Profile badge indicating this user is a member of the Hypesquad House of Brilliance.
    /// </summary>
    HouseBrilliance = 1 << 7,

    /// <summary>
    /// Profile badge indicating this user is a member of the HypeSquad House of Balance.
    /// </summary>
    HouseBalance = 1 << 8,

    /// <summary>
    /// Profile badge indicating this user has purchased Nitro before 10/10/2018.
    /// </summary>
    EarlySupporter = 1 << 9,

    /// <summary>
    /// Profile badge indicating... what, exactly? Discord doesn't tell us.
    /// </summary>
    TeamUser = 1 << 10, 

    /// <summary>
    /// Bug hunter badge, Level 2
    /// </summary>
    BugHunterLevel2 = 1 << 14, 

    /// <summary>
    /// Profile badge indicating this bot is a verified bot.
    /// </summary>
    VerifiedBot = 1 << 16,

    /// <summary>
    /// Profile badge indicating this user has developed a bot which obtained verification before Discord 
    /// stopped giving out the badge.
    /// </summary>
    EarlyVerifiedBotDeveloper = 1 << 17,

    /// <summary>
    /// Profile badge indicating this user has passed Discord's Moderator Exam.
    /// </summary>
    DiscordCertifiedModerator = 1 << 18,

    /// <summary>
    /// Bot that uses only HTTP interactions and is thus shown in the online member list.
    /// </summary>
    BotHttpInteractions = 1 << 19,

    /// <summary>
    /// Profile badge indicating that this user meets the requirements for Discord's active developer badge.
    /// </summary>
    ActiveDeveloper = 1 << 22
}
