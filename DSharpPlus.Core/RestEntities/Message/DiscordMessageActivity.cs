using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordMessageActivity
    {
        [JsonPropertyName("type")]
        public DiscordMessageActivityType Type { get; init; }

        [JsonPropertyName("party_id")]
        public Optional<string> PartyId { get; init; }
    }
}
