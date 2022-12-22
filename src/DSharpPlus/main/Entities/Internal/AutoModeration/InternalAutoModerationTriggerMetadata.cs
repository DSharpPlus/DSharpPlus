using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// Additional data used to determine whether a rule should be triggered. 
/// Different fields are relevant based on the value of <see cref="DiscordAutoModerationTriggerType"/>.
/// </summary>
public sealed record InternalAutoModerationTriggerMetadata
{
    /// <summary>
    /// The substrings which will be searched for in content
    /// </summary>
    /// <remarks>
    /// A keyword can be a phrase which contains multiple words. Wildcard symbols can be used to customize how each 
    /// keyword will be matched. See <see href="https://discord.com/developers/docs/resources/auto-moderation#auto-moderation-rule-object-keyword-matching-strategies">keyword matching strategies</see>.
    /// </remarks>
    [JsonPropertyName("keyword_filter")]
    public required IReadOnlyList<string> KeywordFilter { get; init; } 

    /// <summary>
    /// The internally pre-defined wordsets which will be searched for in content.
    /// </summary>
    [JsonPropertyName("presets")]
    public required DiscordAutoModerationKeywordPresetType PresetType { get; init; }
}
