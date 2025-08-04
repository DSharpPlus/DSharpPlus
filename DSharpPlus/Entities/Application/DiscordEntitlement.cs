using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Entitlement owned by a user or guild
/// </summary>
public sealed class DiscordEntitlement
{
    /// <summary>
    /// ID of the entitlement
    /// </summary>
    [JsonProperty("id")]
    public ulong Id { get; internal set; }
    
    /// <summary>
    /// ID of the SKU
    /// </summary>
    [JsonProperty("sku_id")]
    public ulong StockKeepingUnitId { get; internal set; }
    
    /// <summary>
    /// ID of the parent application
    /// </summary>
    [JsonProperty("application_id")]
    public ulong ApplicationId { get; internal set; }
    
    /// <summary>
    /// ID of the user that is granted access to the entitlement's sku
    /// </summary>
    [JsonProperty("user_id")]
    public ulong? UserId { get; internal set; }
    
    /// <summary>
    /// Type of entitlement
    /// </summary>
    [JsonProperty("type")]
    public DiscordEntitlementType Type { get; internal set; }
    
    /// <summary>
    /// Entitlement was deleted
    /// </summary>
    [JsonProperty("deleted")]
    public bool Deleted { get; internal set; }
    
    /// <summary>
    /// Start date at which the entitlement is valid. Not present when using test entitlements.
    /// </summary>
    [JsonProperty("starts_at")]
    public DateTimeOffset? StartsAt { get; internal set; }
    
    /// <summary>
    /// Date at which the entitlement is no longer valid. Not present when using test entitlements.
    /// </summary>
    [JsonProperty("ends_at")]
    public DateTimeOffset? EndsAt { get; internal set; }
    
    /// <summary>
    /// ID of the guild that is granted access to the entitlement's sku
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong? GuildId { get; internal set; }
    
    /// <summary>
    /// For consumable items, whether the entitlement has been consumed
    /// </summary>
    [JsonProperty("consumed")]
    public bool? Consumed { get; internal set; }
}
