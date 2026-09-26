namespace DSharpPlus.Interactivity.InteractiveMessages;

// Quo usque tandem abutere, automatorum inventores, patientia nostra? quam diu etiam furor iste tuus nos eludet?
// quem ad finem sese effrenata iactabit audacia? [...] O tempora, o mores!
//
// much to my chagrin, too many people voted to keep this to yet justify removing (2026-07), but we can revisit it
// https://canary.discord.com/channels/379378609942560770/379386730538860554/1521239470706266193
//
/// <summary>
/// Provides a mechanism to specify which legacy features to enable. Legacy features will be enabled indiscriminately for all save
/// manually customized interactivity, and DSharpPlus makes no guarantee that all interactivity features work with this.
/// </summary>
public enum LegacyFeatures
{
    /// <summary>
    /// No legacy features should be enabled. This is the default.
    /// </summary>
    None,
    
    /// <summary>
    /// DSharpPlus should prefer classic messages over messages using components v2.
    /// </summary>
    UseV1Messages,

    /// <summary>
    /// DSharpPlus should prefer using reactions over buttons for interactivity. This is likely to be a liability for ratelimits,
    /// and implies <see cref="UseV1Messages"/>. 
    /// </summary>
    UseReactionInteractivity
}
