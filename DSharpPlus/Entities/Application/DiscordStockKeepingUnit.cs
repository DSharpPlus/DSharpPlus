using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// SKUs (stock-keeping units) in Discord represent premium offerings that can be made available to your application's users or guilds.
/// </summary>
public sealed class DiscordStockKeepingUnit
{
    /// <summary>
    /// Id of this entity
    /// </summary>
    [JsonProperty("id")]
    public ulong Id { get; internal set; }
    
    /// <summary>
    /// Type of stock keeping unit
    /// </summary>
    [JsonProperty("type")]
    public DiscordStockKeepingUnitType Type { get; internal set; }
    
    /// <summary>
    /// ID of the parent application
    /// </summary>
    [JsonProperty("application_id")]
    public ulong ApplicationId { get; internal set; }
    
    /// <summary>
    /// Customer-facing name of your premium offering
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }
    
    /// <summary>
    /// System-generated URL slug based on the SKU's name
    /// </summary>
    [JsonProperty("slug")]
    public string Slug { get; internal set; }
    
    /// <summary>
    /// Stock keeping unit flags
    /// </summary>
    [JsonProperty("flags")]
    public DiscordStockKeepingUnitFlags Flags { get; internal set; }
}
