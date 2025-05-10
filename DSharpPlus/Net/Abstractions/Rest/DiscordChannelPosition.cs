using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions.Rest;

public sealed class DiscordChannelPosition
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ChannelId { get; set; }

    [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
    public int Position { get; set; }

    [JsonProperty("lock_permissions", NullValueHandling = NullValueHandling.Ignore)]
    public bool? LockPermissions { get; set; }

    [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? ParentId { get; set; }
}
