using DSharpPlus.Core.Enums;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities
{
    /// <summary>
    /// Sent when an rule is triggered and an action is executed (e.g. message is blocked).
    /// </summary>
    public sealed record DiscordAutoModerationActionExecutionPayload
    {
        /// <summary>
        /// The id of the guild in which action was executed.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The action which was executed.
        /// </summary>
        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordAutoModerationAction Action { get; init; } = null!;

        /// <summary>
        /// The id of the rule which action belongs to.
        /// </summary>
        [JsonProperty("rule_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake RuleId { get; init; } = null!;

        /// <summary>
        /// The trigger type of rule which was triggered.
        /// </summary>
        [JsonProperty("rule_trigger_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordAutoModerationTriggerType RuleTriggerType { get; init; }

        /// <summary>
        /// The id of the user which generated the content which triggered the rule.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The id of the channel in which user content was posted.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> ChannelId { get; init; }

        /// <summary>
        /// The id of any user message which content belongs to.
        /// </summary>
        /// <remarks>
        /// This will be empty if the message was blocked by automod or content was not part of any message.
        /// </remarks>
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> MessageId { get; init; }

        /// <summary>
        /// The id of any system auto moderation messages posted as a result of this action.
        /// </summary>
        /// <remarks>
        /// This will be empty if this event does not correspond to an action with type <see cref="DiscordAutoModerationActionType.SendAlertMessage"/>.
        /// </remarks>
        [JsonProperty("alert_system_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> AlertSystemMessageId { get; init; }

        /// <summary>
        /// The user generated text content.
        /// </summary>
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; init; } = null!;

        /// <summary>
        /// The word or phrase configured in the rule that triggered the rule.
        /// </summary>
        [JsonProperty("matched_keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string? MatchedKeyword { get; init; }

        /// <summary>
        /// The substring in content that triggered the rule.
        /// </summary>
        [JsonProperty("matched_content", NullValueHandling = NullValueHandling.Ignore)]
        public string? MatchedContent { get; init; }
    }
}
