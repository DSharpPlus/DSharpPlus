using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal class RestCreateTestEntitlementPayload
{
    [JsonProperty("sku_id")]
    public ulong SkuId { get; set; }
    
    [JsonProperty("owner_id")]
    public ulong OwnerId { get; set; }
    
    [JsonProperty("owner_type")]
    public DiscordTestEntitlementOwnerType OwnerType { get; set; }
}
