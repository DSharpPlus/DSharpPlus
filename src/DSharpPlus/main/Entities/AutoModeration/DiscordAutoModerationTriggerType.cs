namespace DSharpPlus.Entities;

/// <summary>
/// Characterizes the type of content which can trigger the rule.
/// </summary>
public enum DiscordAutoModerationTriggerType
{
    /// <summary>
    /// Check if content contains words from a user defined list of keywords.
    /// </summary>
    Keyword = 1,

    /// <summary>
    /// Check if content contains any harmful links.
    /// </summary>
    HarmfulLink = 2,

    /// <summary>
    /// Check if content represents generic spam.
    /// </summary>
    Spam = 3,

    /// <summary>
    /// Check if content contains words from internal pre-defined wordsets.
    /// </summary>
    KeywordPreset = 4
}
