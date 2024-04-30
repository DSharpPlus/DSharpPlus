namespace DSharpPlus.Entities.AutoModeration;
using System;
using System.Collections.Generic;

public sealed class DiscordRuleTriggerMetadataBuilder
{
    /// <summary>
    /// Sets substrings which will be searched in the content.
    /// </summary>
    public IReadOnlyList<string>? KeywordFilter { get; private set; }

    /// <summary>
    /// Sets regex patterns which will be matched against the content.
    /// </summary>
    public IReadOnlyList<string>? RegexPatterns { get; private set; }

    /// <summary>
    /// Sets the internally pre-defined wordsets which will be searched in the content.
    /// </summary>
    public IReadOnlyList<DiscordRuleKeywordPresetType>? KeywordPresetTypes { get; private set; }

    /// <summary>
    /// Sets the substrings which should not trigger the rule.
    /// </summary>
    public IReadOnlyList<string>? AllowedKeywords { get; private set; }

    /// <summary>
    /// Sets the total number of mentions (users and roles) allowed per message.
    /// </summary>
    public int? MentionTotalLimit { get; private set; }

    /// <summary>
    /// Sets keywords that will be searched in messages content.
    /// </summary>
    /// <param name="keywordFilter">The keywords that will be searched.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentException"></exception>
    public DiscordRuleTriggerMetadataBuilder AddKeywordFilter(IReadOnlyList<string> keywordFilter)
    {
        if (keywordFilter.Count > 1000)
        {
            throw new ArgumentException("Keyword filter can't contains more than 1000 substrings.");
        }

        KeywordFilter = keywordFilter;

        return this;
    }

    /// <summary>
    /// Sets the regex patterns.
    /// </summary>
    /// <param name="regexPatterns"></param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentException"></exception>
    public DiscordRuleTriggerMetadataBuilder AddRegexPatterns(IReadOnlyList<string> regexPatterns)
    {
        if (regexPatterns.Count > 10)
        {
            throw new ArgumentException("Regex patterns count can't be higher than 10.");
        }

        RegexPatterns = regexPatterns;

        return this;
    }

    /// <summary>
    /// Sets the rule keyword preset types.
    /// </summary>
    /// <param name="keywordPresetTypes">The rule keyword preset types to set.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public DiscordRuleTriggerMetadataBuilder AddKeywordPresetTypes(IReadOnlyList<DiscordRuleKeywordPresetType> keywordPresetTypes)
    {
        KeywordPresetTypes = keywordPresetTypes ?? throw new ArgumentNullException(nameof(keywordPresetTypes));

        return this;
    }

    /// <summary>
    /// Sets an allowed keyword list.
    /// </summary>
    /// <param name="allowList">The keyword list to set.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentException"></exception>
    public DiscordRuleTriggerMetadataBuilder AddAllowedKeywordList(IReadOnlyList<string> allowList)
    {
        if (allowList.Count > 100)
        {
            throw new ArgumentException("Allowed keyword count can't be higher than 100.");
        }

        AllowedKeywords = allowList;

        return this;
    }

    /// <summary>
    /// Sets the total mention limit.
    /// </summary>
    /// <param name="mentionTotalLimit">The total mention limit number.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentException"></exception>
    public DiscordRuleTriggerMetadataBuilder WithMentionTotalLimit(int? mentionTotalLimit)
    {
        if (mentionTotalLimit > 50)
        {
            throw new ArgumentException("Mention total limit can't be higher than 50.");
        }

        MentionTotalLimit = mentionTotalLimit;

        return this;
    }

    /// <summary>
    /// Constructs a new trigger rule metadata.
    /// </summary>
    /// <returns>The build trigger metadata.</returns>
    public DiscordRuleTriggerMetadata Build()
    {
        DiscordRuleTriggerMetadata metadata = new()
        {
            AllowedKeywords = AllowedKeywords ?? Array.Empty<string>(),
            KeywordFilter = KeywordFilter,
            KeywordPresetTypes = KeywordPresetTypes,
            MentionTotalLimit = MentionTotalLimit,
            RegexPatterns = RegexPatterns ?? Array.Empty<string>()
        };

        return metadata;
    }
}
