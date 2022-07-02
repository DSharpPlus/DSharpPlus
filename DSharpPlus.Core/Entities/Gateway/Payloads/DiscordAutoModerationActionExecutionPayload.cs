using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.Entities;

namespace DSharpPlus.Core.Entities.Gateway
{
    /// <summary>
    /// Sent when an rule is triggered and an action is executed (e.g. message is blocked).
    /// </summary>
    public sealed record DiscordAutoModerationActionExecutionPayload
    {
        /// <summary>
        /// The id of the guild in which action was executed.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The action which was executed.
        /// </summary>
        [JsonPropertyName("action")]
        public DiscordAutoModerationAction Action { get; init; } = null!;

        /// <summary>
        /// The id of the rule which action belongs to.
        /// </summary>
        [JsonPropertyName("rule_id")]
        public DiscordSnowflake RuleId { get; init; } = null!;

        /// <summary>
        /// The trigger type of rule which was triggered.
        /// </summary>
        [JsonPropertyName("rule_trigger_type")]
        public DiscordAutoModerationTriggerType RuleTriggerType { get; init; }

        /// <summary>
        /// The id of the user which generated the content which triggered the rule.
        /// </summary>
        [JsonPropertyName("user_id")]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The id of the channel in which user content was posted.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public Optional<DiscordSnowflake> ChannelId { get; init; }

        /// <summary>
        /// The id of any user message which content belongs to.
        /// </summary>
        /// <remarks>
        /// This will be empty if the message was blocked by automod or content was not part of any message.
        /// </remarks>
        [JsonPropertyName("message_id")]
        public Optional<DiscordSnowflake> MessageId { get; init; }

        /// <summary>
        /// The id of any system auto moderation messages posted as a result of this action.
        /// </summary>
        /// <remarks>
        /// This will be empty if this event does not correspond to an action with type <see cref="DiscordAutoModerationActionType.SendAlertMessage"/>.
        /// </remarks>
        [JsonPropertyName("alert_system_message_id")]
        public Optional<DiscordSnowflake> AlertSystemMessageId { get; init; }

        /// <summary>
        /// The user generated text content. Requires the <see cref="DiscordGatewayIntents."/> intent.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; init; } = null!;

        /// <summary>
        /// The word or phrase configured in the rule that triggered the rule.
        /// </summary>
        [JsonPropertyName("matched_keyword")]
        public string? MatchedKeyword { get; init; }

        /// <summary>
        /// The substring in content that triggered the rule.
        /// </summary>
        [JsonPropertyName("matched_content")]
        public string? MatchedContent { get; init; }
    }
}
