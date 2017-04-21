using Newtonsoft.Json;

namespace DSharpPlus.Test
{
    internal sealed class TestBotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; } = string.Empty;

        [JsonProperty("command_prefix")]
        public string CommandPrefix { get; private set; } = "d#+";
    }
}
