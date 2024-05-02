using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal class RestStickerCreatePayload
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public string Tags { get; set; }
}

internal class RestStickerModifyPayload
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public Optional<string> Name { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public Optional<string> Description { get; set; }

    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public Optional<string> Tags { get; set; }
}
