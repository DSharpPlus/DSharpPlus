using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents metadata about the triggering of a Discord rule.
/// </summary>
public sealed class DiscordRuleTriggerMetadata
{
    /// <summary>
    /// Gets substrings which will be searched in the content.
    /// </summary>
    [JsonProperty("keyword_filter", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string>? KeywordFilter { get; internal set; }

    /// <summary>
    /// Gets regex patterns which will be matched against the content.
    /// </summary>
    [JsonProperty("regex_patterns")]
    public IReadOnlyList<string>? RegexPatterns { get; internal set; }

    /// <summary>
    /// Gets the internally pre-defined wordsets which will be searched in the content.
    /// </summary>
    [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordRuleKeywordPresetType>? KeywordPresetTypes { get; internal set; }

    /// <summary>
    /// Gets the substrings which should not trigger the rule.
    /// </summary>
    [JsonProperty("allow_list")]
    public IReadOnlyList<string>? AllowedKeywords { get; internal set; }

    /// <summary>
    /// Gets the total number of mentions (users and roles) allowed per message.
    /// </summary>
    [JsonProperty("mention_total_limit", NullValueHandling = NullValueHandling.Ignore)]
    public int? MentionTotalLimit { get; internal set; }

    internal DiscordRuleTriggerMetadata() { }
}

