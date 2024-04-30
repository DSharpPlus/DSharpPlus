namespace DSharpPlus.Entities;

/// <summary>
/// Characterizes which type of category a blocked word belongs to.
/// </summary>
public enum DiscordRuleKeywordPresetType
{
    /// <summary>
    /// Words that may be considered forms of swearing or cursing.
    /// </summary>
    Profanity = 1,

    /// <summary>
    /// Words that refer to sexually explicit behavior or activity.
    /// </summary>
    SexualContent = 2,

    /// <summary>
    /// Personal insults or words that may be considered hate speech.
    /// </summary>
    Slurs = 3
}
