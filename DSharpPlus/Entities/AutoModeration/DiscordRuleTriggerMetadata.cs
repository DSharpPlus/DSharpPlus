using System;
using System.Collections.Generic;

using DSharpPlus.Enums;

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
    public IReadOnlyList<RuleKeywordPresetType> KeywordPresetTypes { get; internal set; }

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

    internal DiscordRuleTriggerMetadata()
    {

    }
}

public sealed class DiscordRuleTriggerMetadataBuilder
{
    /// <summary>
    /// Sets substrings which will be searched in the content.
    /// </summary>
    public IReadOnlyList<string>? KeywordFilter { get; set; }

    /// <summary>
    /// Sets regex patterns which will be matched against the content.
    /// </summary>
    public IReadOnlyList<string>? RegexPatterns { get; set; }

    /// <summary>
    /// Sets the internally pre-defined wordsets which will be searched in the content.
    /// </summary>
    public IReadOnlyList<RuleKeywordPresetType>? KeywordPresetTypes { get; set; }

    /// <summary>
    /// Sets the substrings which should not trigger the rule.
    /// </summary>
    public IReadOnlyList<string>? AllowedKeywords { get; set; }

    /// <summary>
    /// Sets the total number of mentions (users and roles) allowed per message.
    /// </summary>
    public int? MentionTotalLimit { get; set; }

    public DiscordRuleTriggerMetadataBuilder AddKeywordFilter(IReadOnlyList<string> keywordFilter)
    {
        if (keywordFilter.Count > 1000)
        {
            throw new ArgumentException("Keyword filter can't contains more than 1000 substrings.");
        }

        this.KeywordFilter = keywordFilter;

        return this;
    }

    public DiscordRuleTriggerMetadataBuilder AddRegexPatterns(IReadOnlyList<string> regexPatterns)
    {
        if (regexPatterns.Count > 10)
        {
            throw new ArgumentException("Regex patterns count can't be higher than 10.");
        }

        this.RegexPatterns = regexPatterns;

        return this;
    }

    public DiscordRuleTriggerMetadataBuilder AddKeywordPresetTypes(IReadOnlyList<RuleKeywordPresetType> keywordPresetTypes)
    {
        if (keywordPresetTypes is null)
        {
            throw new ArgumentNullException("Argument can't be null.");
        }

        this.KeywordPresetTypes = keywordPresetTypes;

        return this;
    }

    public DiscordRuleTriggerMetadataBuilder AddAllowedKeywordList(IReadOnlyList<string> allowList)
    {
        if (allowList.Count > 100)
        {
            throw new ArgumentException("Allowed keyword count can't be higher than 100.");
        }

        this.AllowedKeywords = allowList;

        return this;
    }

    public DiscordRuleTriggerMetadataBuilder WithMentionTotalLimit(int? mentionTotalLimit)
    {
        if (mentionTotalLimit > 50)
        {
            throw new ArgumentException("Mention total limit can't be higher than 50.");
        }

        this.MentionTotalLimit = mentionTotalLimit;

        return this;
    }

    public DiscordRuleTriggerMetadata Build()
    {
        var metadata = new DiscordRuleTriggerMetadata
        {
            AllowedKeywords = this.AllowedKeywords ?? Array.Empty<string>(),
            KeywordFilter = this.KeywordFilter,
            KeywordPresetTypes = this.KeywordPresetTypes,
            MentionTotalLimit = this.MentionTotalLimit,
            RegexPatterns = this.RegexPatterns ?? Array.Empty<string>()
        };

        return metadata;
    }
}
