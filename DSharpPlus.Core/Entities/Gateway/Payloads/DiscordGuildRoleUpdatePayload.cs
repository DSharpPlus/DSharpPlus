using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a guild role is updated.
    /// </summary>
    [DiscordGatewayPayload("GUILD_ROLE_UPDATE")]
    public sealed record DiscordGuildRoleUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The role updated.
        /// </summary>
        [JsonPropertyName("role")]
        public DiscordRole Role { get; init; } = null!;
    }
}
