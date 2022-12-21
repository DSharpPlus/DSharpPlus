using System;

namespace DSharpPlus.Entities;

public enum DiscordStageInstancePrivacyLevel
{
    /// <summary>
    /// The Stage instance is visible publicly. (deprecated)
    /// </summary>
    [Obsolete("The Stage instance is visible publicly. (deprecated)")]
    Public = 1,

    /// <summary>
    /// The Stage instance is visible to only guild members.
    /// </summary>
    GuildOnly = 2
}
