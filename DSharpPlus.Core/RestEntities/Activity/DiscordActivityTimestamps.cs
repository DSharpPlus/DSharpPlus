using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivityTimestamps
    {
        /// <summary>
        /// The unix time (in milliseconds) of when the activity started.
        /// </summary>
        [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Start { get; init; }

        /// <summary>
        /// The unix time (in milliseconds) of when the activity ends.
        /// </summary>
        [JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> End { get; init; }
    }
}
