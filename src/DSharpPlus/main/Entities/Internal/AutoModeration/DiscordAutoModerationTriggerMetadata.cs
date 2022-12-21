using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Additional data used to determine whether a rule should be triggered. Different fields are relevant based on the value of <see cref="DiscordGuildAutoModerationTriggerType"/>.
    /// </summary>
    public sealed record DiscordAutoModerationTriggerMetadata
    {
        /// <summary>
        /// The substrings which will be searched for in content
        /// </summary>
        /// <remarks>
        /// A keyword can be a phrase which contains multiple words. Wildcard symbols can be used to customize how each keyword will be matched. See <see href="https://discord.com/developers/docs/resources/auto-moderation#auto-moderation-rule-object-keyword-matching-strategies">keyword matching strategies</see>.
        /// </remarks>
        [JsonPropertyName("keyword_filter")]
        public IReadOnlyList<string> KeywordFilter { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The internally pre-defined wordsets which will be searched for in content.
        /// </summary>
        [JsonPropertyName("presets")]
        public DiscordAutoModerationKeywordPresetType PresetType { get; init; }
    }
}
