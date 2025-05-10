using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions.Rest;

public sealed class DiscordRolePosition
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong RoleId { get; set; }

    [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
    public int Position { get; set; }
}
