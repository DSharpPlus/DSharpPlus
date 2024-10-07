namespace DSharpPlus.Entities;

/// <summary>
/// For subscriptions, SKUs will have a type of either <c>Subscription</c> represented by <c>type: 5</c> or
/// <c>SubscriptionGroup</c> represented by <c>type:6</c> .
/// <br/>
/// For any current implementations, you will want to use the SKU
/// defined by <c>type: 5</c> . A <c>SubscriptionGroup</c> is automatically created for each <c>Subscription</c> SKU
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
