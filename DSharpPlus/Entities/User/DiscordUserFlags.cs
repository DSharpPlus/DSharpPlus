using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents additional details of a users account.
/// </summary>
[Flags]
public enum DiscordUserFlags
{
    /// <summary>
    /// The user has no flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// The user is a Discord employee.
    /// </summary>
    DiscordEmployee = 1 << 0,

    /// <summary>
    /// The user is a Discord partner.
    /// </summary>
    DiscordPartner = 1 << 1,

    /// <summary>
    /// The user has the HypeSquad badge.
    /// </summary>
    HypeSquadEvents = 1 << 2,

    /// <summary>
    /// The user reached the first bug hunter tier.
    /// </summary>
    BugHunterLevelOne = 1 << 3,

    /// <summary>
    /// The user is a member of house bravery.
    /// </summary>
    HouseBravery = 1 << 6,

    /// <summary>
    /// The user is a member of house brilliance.
    /// </summary>
    HouseBrilliance = 1 << 7,

    /// <summary>
    /// The user is a member of house balance.
    /// </summary>
    HouseBalance = 1 << 8,

    /// <summary>
    /// The user has the early supporter badge.
    /// </summary>
    EarlySupporter = 1 << 9,

    /// <summary>
    /// Whether the user is apart of a Discord developer team.
    /// </summary>
    TeamUser = 1 << 10,

    /// <summary>
    /// The user reached the second bug hunter tier.
    /// </summary>
    BugHunterLevelTwo = 1 << 14,

    /// <summary>
    /// Whether the user is an official system user.
    /// </summary>
    System = 1 << 12,

    /// <summary>
    /// The user is a verified bot.
    /// </summary>
    VerifiedBot = 1 << 16,

    /// <summary>
    /// The user is a verified bot developer.
    /// </summary>
    VerifiedBotDeveloper = 1 << 17,

    /// <summary>
    /// The user is a discord certified moderator.
    /// </summary>
    DiscordCertifiedModerator = 1 << 18,

    /// <summary>
    /// The bot receives interactions via HTTP.
    /// </summary>
    HttpInteractionsBot = 1 << 19,

    /// <summary>
    /// The user is an active bot developer.
    /// </summary>
    ActiveDeveloper = 1 << 22
}
