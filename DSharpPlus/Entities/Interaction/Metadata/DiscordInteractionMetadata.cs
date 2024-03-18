namespace DSharpPlus.Entities;

using System.Collections.Generic;
using Newtonsoft.Json;

public abstract class DiscordInteractionMetadata : SnowflakeObject
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; internal set; }

    [JsonProperty("type")]
    public InteractionType Type { get; internal set; }

    [JsonIgnore]
    public DiscordUser User => this.Discord.GetCachedOrEmptyUserInternal(this.UserId);
    
    [JsonProperty("user_id")]
    internal ulong UserId { get; set; }
    
    [JsonIgnore]
    public IReadOnlyDictionary<ApplicationIntegrationType, ulong> AuthorizingIntegrationOwners => this._authorizingIntegrationOwners;
    
    [JsonProperty("authorizing_integration_owners", NullValueHandling = NullValueHandling.Ignore)]
    private Dictionary<ApplicationIntegrationType, ulong> _authorizingIntegrationOwners;
    
    [JsonProperty("original_response_message_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? OriginalMessageID { get; internal set; }
    
    internal DiscordInteractionMetadata() { }
}
