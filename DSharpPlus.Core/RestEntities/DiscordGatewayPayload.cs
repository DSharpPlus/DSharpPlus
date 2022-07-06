using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGatewayPayload
    {
        [JsonPropertyName("op")]
        public int OpCode { get; init; }

        [JsonPropertyName("d")]
        public object? Data { get; init; }

        /// <remarks>
        /// Null when OpCode is not 0
        /// </remarks>
        [JsonPropertyName("s")]
        public int? SequenceNumber { get; init; }

        /// <remarks>
        /// Null when OpCode is not 0
        /// </remarks>
        [JsonPropertyName("t")]
        public string? EventName { get; init; }
    }
}
