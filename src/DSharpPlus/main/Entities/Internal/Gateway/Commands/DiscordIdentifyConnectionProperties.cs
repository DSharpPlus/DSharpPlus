using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Commands
{
    // TODO: Use a static class that utilizes environment variables or csproj values to determine the current library version.
    public sealed record DiscordIdentifyConnectionProperties
    {
        /// <summary>
        /// The operating system that the software is running on.
        /// </summary>
        [JsonPropertyName("os")]
        public string OS { get; init; } = Environment.OSVersion.Platform.ToString();

        /// <summary>
        /// The currently running name of the Discord library.
        /// </summary>
        [JsonPropertyName("browser")]
        public string Browser { get; init; } = "DSharpPlus.Core 5.0.0";

        /// <summary>
        /// The currently running name of the Discord library.
        /// </summary>
        [JsonPropertyName("device")]
        public string Device { get; init; } = "DSharpPlus.Core 5.0.0";
    }
}
