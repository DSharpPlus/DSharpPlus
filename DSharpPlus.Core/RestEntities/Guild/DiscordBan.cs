using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordBan
    {
        /// <summary>
        /// The reason for the ban.
        /// </summary>
        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string? Reason { get; init; }

        /// <summary>
        /// The banned user.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; init; } = null!;
    }
}
