using DSharpPlus.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public sealed class DiscordRuleTriggerMetadata
{
    [JsonProperty("keyword_filter")]
    public string[]? KeywordFilter { get; internal set; }

    [JsonProperty("regex_patterns")]
    public string[]? RegexPatterns { get; internal set; }

    [JsonProperty("presets")]
    public RuleKeywordPresetType[] KeywordPresetTypes { get; internal set; }

    [JsonProperty("allow_list")]
    public string[] AllowedKeywords { get; internal set; }

    [JsonProperty("mention_total_limit")]
    public int? MentionTotalLimit { get; internal set; }

    internal DiscordRuleTriggerMetadata()
    {

    }
}

public sealed class DiscordRuleTriggerMetadataBuilder
{
    public string[]? KeywordFilter { get; set; }

    public string[]? RegexPatterns { get; set; }

    public RuleKeywordPresetType[] KeywordPresetTypes { get; set; }

    public string[] AllowedKeywords { get; set; }

    public int? MentionTotalLimit { get; set; }

    public DiscordRuleTriggerMetadataBuilder()
    {

    }

    public RuleTriggerMetadataBuilder(string[]? keywordFilter, string[]? regexPatterns, RuleKeywordPresetType[] keywordPresetTypes, string[] allowKeywords, int? mentionTotalLimit)
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
