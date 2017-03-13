using System;
using Newtonsoft.Json;

namespace DSharpPlus.Test
{
    internal sealed class TestBotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("command_prefix")]
        public string CommandPrefix { get; private set; }

        public static TestBotConfig Default { get { return _default.Value; } }
        private static Lazy<TestBotConfig> _default = new Lazy<TestBotConfig>(() => new TestBotConfig { Token = string.Empty, CommandPrefix = "d#+" });

        private TestBotConfig() { }
    }
}
