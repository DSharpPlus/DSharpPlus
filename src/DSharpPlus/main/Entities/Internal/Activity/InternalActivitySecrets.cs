using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalActivitySecrets
    {
        /// <summary>
        /// The secret for joining a party.
        /// </summary>
        [JsonPropertyName("join")]
        public Optional<string> Join { get; init; }

        /// <summary>
        /// The secret for spectating a game.
        /// </summary>
        [JsonPropertyName("spectate")]
        public Optional<string> Spectate { get; init; }

        /// <summary>
        /// The secret for a specific instanced match.
        /// </summary>
        [JsonPropertyName("match")]
        public Optional<string> Match { get; init; }
    }
}
