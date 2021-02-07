using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Entities
{
    public class DiscordGuildWelcomeScreen
    {
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        [JsonProperty("welcome_channels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordGuildWelcomeScreenChannel> WelcomeChannels { get; internal set; }
    }
}
