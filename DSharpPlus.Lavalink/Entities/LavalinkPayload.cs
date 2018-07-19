using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.Entities
{
    internal abstract class LavalinkPayload
    {
        [JsonProperty("op")]
        public string Operation { get; }

        internal LavalinkPayload(string opcode)
        {
            this.Operation = opcode;
        }
    }
}
