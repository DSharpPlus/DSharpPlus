using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions.Rest;

internal sealed record RestCreateSoundboardSoundPayload
{
    [JsonProperty("name")]
    public required string Name { get; set; }
    
    [JsonProperty("sound")]
    public required string Sound { get; set; }
    
    [JsonProperty("volume", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public double? Volume { get; set; }
    
    [JsonProperty("emoji_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public ulong? EmojiId { get; set; }
    
    [JsonProperty("emoji_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? EmojiName { get; set; }
}
