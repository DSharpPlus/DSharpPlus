using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordAutoModerationRule : SnowflakeObject
{
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    [JsonProperty("name")]
    public string Name { get; internal set; }

    [JsonProperty("creator_id")]
    public ulong CreatorId { get; internal set; }

    [JsonProperty("event_type")]
    public RuleEventType EventType { get; internal set; }

    [JsonProperty("trigger_type")]
    public RuleTriggerType TriggerType { get; internal set; }

    [JsonProperty("trigger_metadata")]
    public RuleTriggerMetadata Metadata { get; internal set; }

    [JsonProperty("actions")]
    public DiscordAutoModerationAction[]? Actions { get; internal set; }

    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsEnabled { get; internal set; }

    [JsonProperty("exempt_roles", NullValueHandling = NullValueHandling.Ignore)]
    public ulong[] ExemptRoles { get; internal set; }

    [JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
    public ulong[] ExemptChannels { get; internal set; }

    public Task DeleteAsync(string reason = null)
        => this.Discord.ApiClient.DeleteGuildAutoModerationRuleAsync(this.GuildId, this.Id, reason);

    public Task<DiscordAutoModerationRule> ModifyAsync(ulong rule_id, Optional<string> name = default, Optional<RuleEventType> event_type = default, Optional<RuleTriggerMetadata> trigger_metadata = default, Optional<IEnumerable<DiscordAutoModerationAction>> actions = default, Optional<bool> enabled = default, Optional<IEnumerable<ulong>> exempt_roles = default, Optional<IEnumerable<ulong>> exempt_channels = default, string reason = null)
        => this.Discord.ApiClient.ModifyGuildAutoModerationRuleAsync(this.Id, rule_id, name, event_type, trigger_metadata, actions, enabled, exempt_roles, exempt_channels, reason);
}
