using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol;

internal sealed record RequestChannelInfoData
{
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; init; }

    [JsonPropertyName("fields")]
    public IReadOnlyList<string> Fields => ["voice_start_time"];
}
