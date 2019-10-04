using Newtonsoft.Json;

namespace DSharpPlus.Test
{
    internal sealed class TestBotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; } = string.Empty;

        [JsonProperty("command_prefixes")]
        public string[] CommandPrefixes { get; private set; } = new[] { "d#", "d#+"  };

        [JsonProperty("shards")]
        public int ShardCount { get; private set; } = 1;
    }
}
