namespace DSharpPlus.Entities;

/// <summary>
/// Type of Entitlement
/// </summary>
public enum DiscordEntitlementType
{
    /// <summary>
    /// Entitlement was purchased by user
    /// </summary>
    Purchase,
    
    /// <summary>
    /// Entitlement for Discord Nitro subscription
    /// </summary>
    PremiumSubscription,
    
    /// <summary>
    /// Entitlement was gifted by developer
    /// </summary>
    DeveloperGift,
    
    /// <summary>
    /// Entitlement was purchased by a dev in application test mode
    /// </summary>
    TestModePurchase,
    
    /// <summary>
    /// Entitlement was granted when the SKU was free
    /// </summary>
    FreePurchase,
    
    /// <summary>
    /// Entitlement was gifted by another user
    /// </summary>
    UserGift,
    
    /// <summary>
    /// Entitlement was claimed by user for free as a Nitro Subscriber
    /// </summary>
    PremiumPurchase,
    
    /// <summary>
    /// Entitlement was purchased as an app subscription
    /// </summary>
    ApplicationSubscription
}
