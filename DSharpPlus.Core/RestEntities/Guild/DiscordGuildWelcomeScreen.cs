using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildWelcomeScreen
    {
        /// <summary>
        /// The server description shown in the welcome screen.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; init; }

        /// <summary>
        /// The channels shown in the welcome screen, up to 5.
        /// </summary>
        [JsonPropertyName("welcome_channels")]
        public IReadOnlyList<DiscordGuildWelcomeScreenChannel> WelcomeChannels { get; init; } = Array.Empty<DiscordGuildWelcomeScreenChannel>();
    }
}
