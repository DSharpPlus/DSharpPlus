using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalBan
    {
        /// <summary>
        /// The reason for the ban.
        /// </summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; init; }

        /// <summary>
        /// The banned user.
        /// </summary>
        [JsonPropertyName("user")]
        public InternalUser User { get; init; } = null!;
    }
}
