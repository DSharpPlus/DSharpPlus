using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalIntegrationAccount
    {
        /// <summary>
        /// The id of the account.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; init; } = null!;

        /// <summary>
        /// The name of the account.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;
    }
}
