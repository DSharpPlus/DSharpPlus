using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal sealed class GatewayRequestChannelInfo
{
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    // https://docs.discord.com/developers/events/gateway-events#request-channel-info
    [JsonProperty("fields")]
    public IReadOnlyList<string> Fields { get; internal set; }
}
