using System.Collections.Generic;

using DSharpPlus.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public sealed class DiscordRuleTriggerMetadata
{
    /// <summary>
    /// Gets substrings which will be searched in the content.
    /// </summary>
    [JsonProperty("keyword_filter")]
    public IReadOnlyList<string>? KeywordFilter { get; internal set; }

    /// <summary>
    /// Gets regex patterns which will be matched against the content.
    /// </summary>
    [JsonProperty("regex_patterns")]
    public IReadOnlyList<string>? RegexPatterns { get; internal set; }

    /// <summary>
    /// Gets the internally pre-defined wordsets which will be searched in the content.
    /// </summary>
    [JsonProperty("presets")]
    public IReadOnlyList<RuleKeywordPresetType> KeywordPresetTypes { get; internal set; }

    /// <summary>
    /// Gets the substrings which should not trigger the rule.
    /// </summary>
    [JsonProperty("allow_list")]
    public IReadOnlyList<string> AllowedKeywords { get; internal set; }

    /// <summary>
    /// Gets the total number of mentions (users and roles) allowed per message.
    /// </summary>
    [JsonProperty("mention_total_limit", NullValueHandling = NullValueHandling.Ignore)]
    public int? MentionTotalLimit { get; internal set; }

    internal DiscordRuleTriggerMetadata()
    {

    }
}

public sealed class DiscordRuleTriggerMetadataBuilder
{
    public IReadOnlyList<string>? KeywordFilter { get; set; }

    public IReadOnlyList<string>? RegexPatterns { get; set; }

    public IReadOnlyList<RuleKeywordPresetType> KeywordPresetTypes { get; set; }

    public IReadOnlyList<string> AllowedKeywords { get; set; }

    public int? MentionTotalLimit { get; set; }

    public DiscordRuleTriggerMetadataBuilder()
    {

    }

    public DiscordRuleTriggerMetadataBuilder(IReadOnlyList<string>? keywordFilter, IReadOnlyList<string>? regexPatterns, IReadOnlyList<RuleKeywordPresetType> keywordPresetTypes, IReadOnlyList<string> allowKeywords, int? mentionTotalLimit)
    {
        this.AllowedKeywords = allowKeywords;
        this.MentionTotalLimit = mentionTotalLimit;
        this.RegexPatterns = regexPatterns;
        this.KeywordPresetTypes = keywordPresetTypes;
        this.KeywordFilter = keywordFilter;
    }

    public DiscordRuleTriggerMetadata Build()
    {
        return new DiscordRuleTriggerMetadata
        {
            AllowedKeywords = this.AllowedKeywords,
            KeywordFilter = this.KeywordFilter,
            KeywordPresetTypes = this.KeywordPresetTypes,
            MentionTotalLimit = this.MentionTotalLimit,
            RegexPatterns = this.RegexPatterns
        };
    }
}
