using System;

namespace DSharpPlus.Entities;


[Flags]
public enum DiscordUserFlags
{
    /// <summary>
    /// The user has no flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// The user is a Discord Employee.
    /// </summary>
    Staff = 1 << 0,

    /// <summary>
    /// The user is a Partnered Server Owner.
    /// </summary>
    Partner = 1 << 1,

    /// <summary>
    /// The user is a HypeSquad Events Coordinator.
    /// </summary>
    Hypesquad = 1 << 2,

    /// <summary>
    /// The user is a Level 1 Bug Hunter.
    /// </summary>
    BugHunterLevel1 = 1 << 3,

    /// <summary>
    /// The user is part of the Hypesquad House of Bravery.
    /// </summary>
    HypesquadBravery = 1 << 6,

    /// <summary>
    /// The user is part of the Hypesquad House of Brilliance.
    /// </summary>
    HypesquadBrilliance = 1 << 7,

    /// <summary>
    /// The user is part of the Hypesquad House of Balance.
    /// </summary>
    HypesquadBalance = 1 << 8,

    /// <summary>
    /// The user is Early Nitro Supporter.
    /// </summary>
    NitroEarlySupporter = 1 << 9,

    /// <summary>
    /// The user is a team.
    /// </summary>
    TeamPseudoUser = 1 << 10,

    /// <summary>
    /// The user is a Level 2 Bug Hunter.
    /// </summary>
    BugHunterLevel2 = 1 << 14,

    /// <summary>
    /// The user is a Verified Bot.
    /// </summary>
    VerifiedBot = 1 << 16,

    /// <summary>
    /// The user is a Verified Bot Developer.
    /// </summary>
    VerifiedDeveloper = 1 << 17,

    /// <summary>
    /// The user is a Discord Certified Moderator.
    /// </summary>
    CertifiedModerator = 1 << 18,

    /// <summary>
    /// The user is a bot that uses only HTTP interactions and is shown in the online member list.
    /// </summary>
    BotHttpInteractions = 1 << 19,
}
