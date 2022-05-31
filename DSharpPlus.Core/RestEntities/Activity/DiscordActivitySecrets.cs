using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivitySecrets
    {
        /// <summary>
        /// The secret for joining a party.
        /// </summary>
        [JsonProperty("join", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Join { get; init; }

        /// <summary>
        /// The secret for spectating a game.
        /// </summary>
        [JsonProperty("spectate", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Spectate { get; init; }

        /// <summary>
        /// The secret for a specific instanced match.
        /// </summary>
        [JsonProperty("match", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Match { get; init; }
    }
}
