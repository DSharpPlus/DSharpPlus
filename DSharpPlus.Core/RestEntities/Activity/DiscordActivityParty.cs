using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivityParty
    {
        /// <summary>
        /// The id of the party.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Id { get; init; }

        /// <summary>
        /// Used to show the party's current and maximum size.
        /// </summary>
        /// <remarks>
        /// <c>Size[0]</c> is the current size, and <c>Size[1]</c> is the maximum size.
        /// </remarks>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<int>> Size { get; init; }
    }
}
