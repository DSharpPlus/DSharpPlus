namespace DSharpPlus.Entities;

/// <summary>
/// For subscriptions, SKUs will have a type of either SUBSCRIPTION represented by type: 5 or
/// SUBSCRIPTION_GROUP represented by type:6. For any current implementations, you will want to use the SKU
/// defined by type: 5. A SUBSCRIPTION_GROUP is automatically created for each SUBSCRIPTION SKU
/// and are not used at this time.
/// </summary>
public enum DiscordStockKeepingUnitType
{
    /// <summary>
    /// Durable one-time purchase
    /// </summary>
    Durable = 2,
    
    /// <summary>
    /// Consumable one-time purchase
    /// </summary>
    Consumable = 3,
    
    /// <summary>
    /// Represents a recurring subscription
    /// </summary>
    Subscription = 5,
    
    /// <summary>
    /// System-generated group for each SUBSCRIPTION SKU created
    /// </summary>
    SubscriptionGroup = 6
}
