namespace DSharpPlus.Net.Abstractions;

using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal sealed class AuditLogActionChange
{
    // this can be a string or an array
    [JsonProperty("old_value")]
    public object OldValue { get; set; }

    [JsonIgnore]
    public IEnumerable<JObject> OldValues
        => (OldValue as JArray)?.ToDiscordObject<IEnumerable<JObject>>();

    [JsonIgnore]
    public ulong OldValueUlong
        => (ulong)OldValue;

    [JsonIgnore]
    public string OldValueString
        => (string)OldValue;

    [JsonIgnore]
    public bool OldValueBool
        => (bool)OldValue;

    [JsonIgnore]
    public long OldValueLong
        => (long)OldValue;


    // this can be a string or an array
    [JsonProperty("new_value")]
    public object NewValue { get; set; }

    [JsonIgnore]
    public IEnumerable<JObject> NewValues
        => (NewValue as JArray)?.ToDiscordObject<IEnumerable<JObject>>();

    [JsonIgnore]
    public ulong NewValueUlong
        => (ulong)NewValue;

    [JsonIgnore]
    public string NewValueString
        => (string)NewValue;

    [JsonIgnore]
    public bool NewValueBool
        => (bool)NewValue;

    [JsonIgnore]
    public long NewValueLong
        => (long)NewValue;

    [JsonProperty("key")]
    public string Key { get; set; }
}

internal sealed class AuditLogActionOptions
{
    [JsonProperty("application_id")]
    public ulong ApplicationId { get; set; }

    [JsonProperty("auto_moderation_rule_name")]
    public string AutoModerationRuleName { get; set; }

    [JsonProperty("auto_moderation_rule_trigger_type")]
    public string AutoModerationRuleTriggerType { get; set; }

    [JsonProperty("channel_id")]
    public ulong ChannelId { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("delete_member_days")]
    public int DeleteMemberDays { get; set; }

    [JsonProperty("id")]
    public ulong Id { get; set; }

    [JsonProperty("members_removed")]
    public int MembersRemoved { get; set; }

    [JsonProperty("message_id")]
    public ulong MessageId { get; set; }

    [JsonProperty("role_name")]
    public string RoleName { get; set; }

    [JsonProperty("type")]
    public object Type { get; set; }
}

internal sealed class AuditLogAction
{
    [JsonProperty("target_id")]
    public ulong? TargetId { get; set; }

    [JsonProperty("user_id")]
    public ulong UserId { get; set; }

    [JsonProperty("id")]
    public ulong Id { get; set; }

    [JsonProperty("action_type")]
    public DiscordAuditLogActionType ActionType { get; set; }

    [JsonProperty("changes")]
    public IEnumerable<AuditLogActionChange> Changes { get; set; }

    [JsonProperty("options")]
    public AuditLogActionOptions Options { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }
}

internal sealed class AuditLog
{
    [JsonProperty("application_commands")]
    private IEnumerable<DiscordApplicationCommand> SlashCommands { get; set; }

    [JsonProperty("audit_log_entries")]
    public IEnumerable<AuditLogAction> Entries { get; set; }

    [JsonProperty("auto_moderation_rules")]
    private IEnumerable<DiscordAutoModerationRule> AutoModerationRules { get; set; }

    [JsonProperty("guild_scheduled_events")]
    public IEnumerable<DiscordScheduledGuildEvent> Events { get; set; }

    [JsonProperty("integrations")]
    public IEnumerable<DiscordIntegration> Integrations { get; set; }

    [JsonProperty("threads")]
    public IEnumerable<DiscordThreadChannel> Threads { get; set; }

    [JsonProperty("users")]
    public IEnumerable<DiscordUser> Users { get; set; }

    [JsonProperty("webhooks")]
    public IEnumerable<DiscordWebhook> Webhooks { get; set; }
}
