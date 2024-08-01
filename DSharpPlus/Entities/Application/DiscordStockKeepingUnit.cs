namespace DSharpPlus.Entities;

/// <summary>
/// SKUs (stock-keeping units) in Discord represent premium offerings that can be made available to your application's users or guilds.
/// </summary>
public class DiscordStockKeepingUnit
{
    /// <summary>
    /// Id of this entity
    /// </summary>
    public ulong Id { get; internal set; }
    
    /// <summary>
    /// Type of stock keeping unit
    /// </summary>
    public DiscordStockKeepingUnitType Type { get; internal set; }
    
    /// <summary>
    /// ID of the parent application
    /// </summary>
    public ulong ApplicationId { get; internal set; }
    
    /// <summary>
    /// Customer-facing name of your premium offering
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// System-generated URL slug based on the SKU's name
    /// </summary>
    public string Slug { get; internal set; }
    
    /// <summary>
    /// Stock keeping unit flags
    /// </summary>
    public DiscordStockKeepingUnitFlags Flags { get; internal set; }
}
