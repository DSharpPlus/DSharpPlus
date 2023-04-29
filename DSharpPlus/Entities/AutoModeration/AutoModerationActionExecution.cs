using DSharpPlus.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class AutoModerationActionExecution
{
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    [JsonProperty("action")]
    public DiscordAutoModerationAction Action { get; internal set; }

    [JsonProperty("rule_id")]
    public ulong RuleId { get; internal set; }

    [JsonProperty("rule_trigger_type")]
    public RuleTriggerType TriggerType { get; internal set; }

    [JsonProperty("user_id")]
    public ulong UserId { get; internal set; }

    [JsonProperty("channel_id")]
    public ulong? ChannelId { get; internal set; }

    [JsonProperty("message_id")]
    public ulong? MessageId { get; internal set; }

    [JsonProperty("alert_system_message_id")]
    public ulong LogSystemMessageId { get; internal set; }

    [JsonProperty("content")]
    public string? Content { get; internal set; }

    [JsonProperty("matched_keyword")]
    public string? MatchedKeyword { get; internal set; }

    [JsonProperty("matched_content")]
    public string? MatchedContent { get; internal set; }
}
