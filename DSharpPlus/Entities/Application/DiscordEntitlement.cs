using System;

namespace DSharpPlus.Entities;

public class DiscordEntitlement
{
    /// <summary>
    /// ID of the entitlement
    /// </summary>
    public ulong Id { get; internal set; }
    
    /// <summary>
    /// ID of the SKU
    /// </summary>
    public ulong StockKeepingUnitId { get; internal set; }
    
    /// <summary>
    /// ID of the parent application
    /// </summary>
    public ulong ApplicationId { get; internal set; }
    
    /// <summary>
    /// ID of the user that is granted access to the entitlement's sku
    /// </summary>
    public ulong? UserId { get; internal set; }
    
    public DiscordEntitlementType Type { get; internal set; }
    
    public bool Deleted { get; internal set; }
    
    /// <summary>
    /// Start date at which the entitlement is valid. Not present when using test entitlements.
    /// </summary>
    public DateTimeOffset? StartsAt { get; internal set; }
    
    /// <summary>
    /// Date at which the entitlement is no longer valid. Not present when using test entitlements.
    /// </summary>
    public DateTimeOffset? EndsAt { get; internal set; }
    
    /// <summary>
    /// ID of the guild that is granted access to the entitlement's sku
    /// </summary>
    public ulong? GuildId { get; internal set; }
    
    /// <summary>
    /// For consumable items, whether or not the entitlement has been consumed
    /// </summary>
    public bool? Consumed { get; internal set; }
}
