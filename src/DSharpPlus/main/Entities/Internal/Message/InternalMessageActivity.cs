using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalMessageActivity
    {
        [JsonPropertyName("type")]
        public InternalMessageActivityType Type { get; init; }

        [JsonPropertyName("party_id")]
        public Optional<string> PartyId { get; init; }
    }
}
