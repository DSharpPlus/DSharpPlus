namespace DSharpPlus.Entities;

/// <summary>
/// The type of Nitro subscription on a user's account.
/// </summary>
public enum DiscordPremiumType
{
    /// <summary>
    /// Includes app perks like animated emojis and avatars, but not games.
    /// </summary>
    NitroClassic = 1,
    /// <summary>
    /// Includes app perks as well as the games subscription service.
    /// </summary>
    Nitro = 2
}
