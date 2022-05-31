using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildWelcomeScreen
    {
        /// <summary>
        /// The server description shown in the welcome screen.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; init; }

        /// <summary>
        /// The channels shown in the welcome screen, up to 5.
        /// </summary>
        [JsonProperty("welcome_channels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordGuildWelcomeScreenChannel> WelcomeChannels { get; init; } = Array.Empty<DiscordGuildWelcomeScreenChannel>();
    }
}
