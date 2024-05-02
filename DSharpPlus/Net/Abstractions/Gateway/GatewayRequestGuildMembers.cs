
using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;
internal sealed class GatewayRequestGuildMembers
{
    [JsonProperty("guild_id")]
    public ulong GuildId { get; }

    [JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
    public string Query { get; set; } = null;

    [JsonProperty("limit")]
    public int Limit { get; set; } = 0;

    [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Presences { get; set; } = null;

    [JsonProperty("user_ids", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<ulong> UserIds { get; set; } = null;

    [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
    public string Nonce { get; internal set; }

    public GatewayRequestGuildMembers(DiscordGuild guild) => GuildId = guild.Id;
}
