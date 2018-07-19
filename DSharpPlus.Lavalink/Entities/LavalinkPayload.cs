using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities
{
    internal abstract class LavalinkPayload
    {
        [JsonProperty("op")]
        public string Operation { get; }

        [JsonProperty("guildId")]
        public string GuildId { get; }

        internal LavalinkPayload(string opcode, string guildId)
        {
            this.Operation = opcode;
            this.GuildId = guildId;
        }
    }
}
