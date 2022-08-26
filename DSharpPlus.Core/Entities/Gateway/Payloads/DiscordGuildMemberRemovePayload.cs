using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user is removed from a guild (leave/kick/ban).
    /// </summary>
    [DiscordGatewayPayload("GUILD_MEMBER_REMOVE")]
    public sealed record DiscordGuildMemberRemovePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The user who was removed.
        /// </summary>
        [JsonPropertyName("user")]
        public DiscordUser User { get; init; } = null!;
    }
}
