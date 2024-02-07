using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Caching;
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
    /// Gets the guild which the rule belongs to.
    /// </summary>
    /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
    /// <param name="skipCache">Whether to skip the cache and always excute a REST request</param>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild is not found.</exception>
    public async ValueTask<DiscordGuild> GetGuildAsync(bool skipCache = false, bool withCounts = false)
    {
        if (Discord is DiscordClient dc)
        {
            return await dc.GetGuildAsync(this.GuildId, skipCache, withCounts);
        }

        if (skipCache)
        {
            return await this.Discord.ApiClient.GetGuildAsync(this.GuildId, withCounts);
        }
        
        DiscordGuild? guild = await this.Discord.Cache.TryGet<DiscordGuild>(ICacheKey.ForGuild(this.GuildId));
        if (guild is not null)
        {
            return guild;
        }
        
        return await this.Discord.ApiClient.GetGuildAsync(this.GuildId, withCounts);
    }
    

    /// <summary>
    /// Gets the rule name.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; internal set; }

    [JsonProperty("creator_id")]
    internal ulong CreatorId { get; set; }

    /// <summary>
    /// Gets the user that created the rule.
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always excute a REST request</param>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the user is not found.</exception>
    public async ValueTask<DiscordUser> GetCreatorAsync(bool skipCache = false)
    {
        if (Discord is DiscordClient dc)
        {
            return await dc.GetUserAsync(this.CreatorId, skipCache);
        }

        if (skipCache)
        {
            return await this.Discord.ApiClient.GetUserAsync(this.CreatorId);
        }
            
        
        DiscordUser? user = await this.Discord.Cache.TryGetUserAsync(this.CreatorId);
        if (user is not null)
        {
            return user;
        }
        
        return await this.Discord.ApiClient.GetUserAsync(this.CreatorId);
    }

    /// <summary>
    /// Gets the rule event type.
    /// </summary>
    [JsonProperty("event_type")]
    public DiscordRuleEventType EventType { get; internal set; }

    /// <summary>
    /// Gets the rule trigger type.
    /// </summary>
    [JsonProperty("trigger_type")]
    public DiscordRuleTriggerType TriggerType { get; internal set; }

    /// <summary>
    /// Gets the additional data to determine whether a rule should be triggered.
    /// </summary>
    [JsonProperty("trigger_metadata")]
    public DiscordRuleTriggerMetadata? Metadata { get; internal set; }

    /// <summary>
    /// Gets actions which will execute when the rule is triggered.
    /// </summary>
    [JsonProperty("actions")]
    public IReadOnlyList<DiscordAutoModerationAction>? Actions { get; internal set; }

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
    public IReadOnlyList<ulong>? ExemptRoles { get; internal set; }

    /// <summary>
    /// Gets ids of channels in which rule will be not triggered.
    /// </summary>
    /// <remarks>
    /// Maximum of 50.
    /// </remarks>
    [JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<ulong>? ExemptChannels { get; internal set; }

    /// <summary>
    /// Deletes the rule in the guild.
    /// </summary>
    /// <param name="reason">Reason for audits logs.</param>
    public async Task DeleteAsync(string? reason = null)
        => await this.Discord.ApiClient.DeleteGuildAutoModerationRuleAsync(this.GuildId, this.Id, reason);

    /// <summary>
    /// Modify the rule in the guild.
    /// </summary>
    /// <param name="action">Action the perform on this rule.</param>
    /// <returns>The modified rule.</returns>
    public async Task<DiscordAutoModerationRule> ModifyAsync(Action<AutoModerationRuleEditModel> action)
    {
        AutoModerationRuleEditModel model = new();

        action(model);

        return await this.Discord.ApiClient.ModifyGuildAutoModerationRuleAsync
        (
            this.GuildId,
            this.Id,
            model.Name,
            model.EventType,
            model.TriggerMetadata,
            model.Actions,
            model.Enable,
            model.ExemptRoles,
            model.ExemptChannels,
            model.AuditLogReason
        );
    }
}
