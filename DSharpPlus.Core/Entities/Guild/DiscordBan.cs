using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordBan
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
        public DiscordUser User { get; init; } = null!;
    }
}
