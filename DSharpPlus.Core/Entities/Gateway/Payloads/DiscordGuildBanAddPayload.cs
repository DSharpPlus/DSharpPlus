using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user is banned from a guild.
    /// </summary>
    [DiscordGatewayPayload("GUILD_BAN_ADD")]
    public sealed record DiscordGuildBanAddPayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The banned user.
        /// </summary>
        [JsonPropertyName("user")]
        public DiscordUser User { get; init; } = null!;
    }
}
