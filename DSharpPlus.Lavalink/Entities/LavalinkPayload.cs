namespace DSharpPlus.Lavalink.Entities;
using Newtonsoft.Json;

internal abstract class LavalinkPayload
{
    [JsonProperty("op")]
    public string Operation { get; }

    [JsonProperty("guildId", NullValueHandling = NullValueHandling.Ignore)]
    public string GuildId { get; }

    internal LavalinkPayload(string opcode) => Operation = opcode;

    internal LavalinkPayload(string opcode, string guildId)
    {
        Operation = opcode;
        GuildId = guildId;
    }
}
