using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Enums;
using DSharpPlus.Net.Models;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord auto-moderation rule.
/// </summary>
public class DiscordAutoModerationRule : SnowflakeObject
{
    [JsonProperty("guild_id")]
    internal ulong GuildId { get; set; }

    /// <summary>
    /// Gets the guild which the rule is in.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild? Guild => this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;

    /// <summary>
    /// Gets the rule name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the id of the user that created the rule.
    /// </summary>
    [JsonProperty("creator_id")]
    public ulong CreatorId { get; internal set; }

    /// <summary>
    /// Gets the rule event type.
    /// </summary>
    [JsonProperty("event_type")]
    public RuleEventType EventType { get; internal set; }

    /// <summary>
    /// Gets the rule trigger type.
    /// </summary>
    [JsonProperty("trigger_type")]
    public RuleTriggerType TriggerType { get; internal set; }

    /// <summary>
    /// Gets the additional data to determine whether a rule should be triggered.
    /// </summary>
    [JsonProperty("trigger_metadata")]
    public DiscordRuleTriggerMetadata Metadata { get; internal set; }

    /// <summary>
    /// Gets actions which will execute when the rule is triggered.
    /// </summary>
    [JsonProperty("actions")]
    public IReadOnlyList<DiscordAutoModerationAction> Actions { get; internal set; }

    /// <summary>
    /// Gets whether the rule is enabled.
    /// </summary>
    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsEnabled { get; internal set; }

    /// <summary>
    /// Gets ids of roles that will not trigger the rule.
    /// </summary>
    /// <remarks>
    /// Maximum of 20.
    /// </remarks>
    [JsonProperty("exempt_roles", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<ulong> ExemptRoles { get; internal set; }

    /// <summary>
    /// Gets ids of channels in which rule will be not triggered.
    /// </summary>
    /// <remarks>
    /// Maximum of 50.
    /// </remarks>
    [JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<ulong> ExemptChannels { get; internal set; }

    /// <summary>
    /// Deletes the rule in the guild.
    /// </summary>
    /// <param name="reason">Reason for audits logs.</param>
    public Task DeleteAsync(string reason = null)
        => this.Discord.ApiClient.DeleteGuildAutoModerationRuleAsync(this.GuildId, this.Id, reason);

    /// <summary>
    /// Modify the rule in the guild.
    /// </summary>
    /// <param name="action">Action the perform on this rule.</param>
    /// <returns>The modified rule.</returns>
    public async Task<DiscordAutoModerationRule> ModifyAsync(Action<AutoModerationRuleEditModel> action)
    {
        var model = new AutoModerationRuleEditModel();

        action(model);

        return await this.Discord.ApiClient.ModifyGuildAutoModerationRuleAsync(this.GuildId, this.Id, model.Name, model.EventType, model.TriggerMetadata, model.Actions, model.Enable, model.ExemptRoles, model.ExemptChannels, model.AuditLogReason);
    }
}
