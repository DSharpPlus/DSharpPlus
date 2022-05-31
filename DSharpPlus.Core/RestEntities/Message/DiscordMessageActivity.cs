using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordMessageActivity
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMessageActivityType Type { get; init; }

        [JsonProperty("party_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> PartyId { get; init; }
    }
}
