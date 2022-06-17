using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivityParty
    {
        /// <summary>
        /// The id of the party.
        /// </summary>
        [JsonPropertyName("id")]
        public Optional<string> Id { get; init; }

        /// <summary>
        /// Used to show the party's current and maximum size.
        /// </summary>
        /// <remarks>
        /// <c>Size[0]</c> is the current size, and <c>Size[1]</c> is the maximum size.
        /// </remarks>
        [JsonPropertyName("size")]
        public Optional<IReadOnlyList<int>> Size { get; init; }
    }
}
