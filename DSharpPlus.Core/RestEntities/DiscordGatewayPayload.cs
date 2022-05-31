using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGatewayPayload
    {
        [JsonProperty("op", NullValueHandling = NullValueHandling.Ignore)]
        public int OpCode { get; init; }

        [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)]
        public object? Data { get; init; }

        /// <remarks>
        /// Null when OpCode is not 0
        /// </remarks>
        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
        public int? SequenceNumber { get; init; }

        /// <remarks>
        /// Null when OpCode is not 0
        /// </remarks>
        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string? EventName { get; init; }
    }
}
