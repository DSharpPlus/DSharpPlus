using System;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Commands
{
    // TODO: Use a static class that utilizes environment variables or csproj values to determine the current library version.
    public sealed record DiscordIdentifyConnectionProperties
    {
        /// <summary>
        /// The operating system that the software is running on.
        /// </summary>
        [JsonProperty("os", NullValueHandling = NullValueHandling.Ignore)]
        public string OS { get; init; } = Environment.OSVersion.Platform.ToString();

        /// <summary>
        /// The currently running name of the Discord library.
        /// </summary>
        [JsonProperty("browser", NullValueHandling = NullValueHandling.Ignore)]
        public string Browser { get; init; } = "DSharpPlus.Core 5.0.0";

        /// <summary>
        /// The currently running name of the Discord library.
        /// </summary>
        [JsonProperty("device", NullValueHandling = NullValueHandling.Ignore)]
        public string Device { get; init; } = "DSharpPlus.Core 5.0.0";
    }
}
