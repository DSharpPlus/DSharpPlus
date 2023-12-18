namespace DSharpPlus.Entities;

/// <summary>
/// Characterizes the type of content which can trigger a rule.
/// </summary>
public enum DiscordRuleTriggerType
{
    /// <summary>
    /// Check if the content contains words from a definied list of keywords.
    /// </summary>
    Keyword = 1,

    /// <summary>
    /// Check if the content is a spam.
    /// </summary>
    Spam = 3,

    /// <summary>
    /// Check if the content contains words from pre-defined wordsets.
    /// </summary>
    KeywordPreset = 4,

    /// <summary>
    /// Check if the content contains moure unique mentions than allowed.
    /// </summary>
    MentionSpam = 5,
}
