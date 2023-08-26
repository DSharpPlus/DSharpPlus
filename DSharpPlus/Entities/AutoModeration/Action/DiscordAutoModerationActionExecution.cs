namespace DSharpPlus.Entities;

using Enums;

using Newtonsoft.Json;

/// <summary>
/// Represents a Discord rule executed action.
/// </summary>
public class DiscordAutoModerationActionExecution
{
    /// <summary>
    /// Gets the id of the guild in which action was executed.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Gets the action which was executed.
    /// </summary>
    [JsonProperty("action")]
    public DiscordAutoModerationAction Action { get; internal set; }

    /// <summary>
    /// Gets the id of the rule which was triggered.
    /// </summary>
    [JsonProperty("rule_id")]
    public ulong RuleId { get; internal set; }

    /// <summary>
    /// Gets the rule trigger type.
    /// </summary>
    [JsonProperty("rule_trigger_type")]
    public RuleTriggerType TriggerType { get; internal set; }

    /// <summary>
    /// Gets the id of the user which triggered the rule.
    /// </summary>
    [JsonProperty("user_id")]
    public ulong UserId { get; internal set; }

    /// <summary>
    /// Gets the id of the channel in which user triggered the rule.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? ChannelId { get; internal set; }

    /// <summary>
    /// Gets the id of any user message which content belongs to.
    /// </summary>
    [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? MessageId { get; internal set; }

    /// <summary>
    /// Gets the id of the message sent by the alert system.
    /// </summary>
    [JsonProperty("alert_system_message_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? AlertSystemMessageId { get; internal set; }

    /// <summary>
    /// Gets the content of the message.
    /// </summary>
    /// <remarks><see cref="DiscordIntents.MessageContents"/> is required to not get an empty value.</remarks>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public string? Content { get; internal set; }

    /// <summary>
    /// Gets the keywords (word or phrase) configured in the rule that triggered it.
    /// </summary>
    [JsonProperty("matched_keyword")]
    public string MatchedKeyword { get; internal set; }

    /// <summary>
    /// Gets the substring in content that triggered the rule.
    /// </summary>
    /// <remarks><see cref="DiscordIntents.MessageContents"/> is required to not get an empty value.</remarks>
    [JsonProperty("matched_content", NullValueHandling = NullValueHandling.Ignore)]
    public string? MatchedContent { get; internal set; }
}
