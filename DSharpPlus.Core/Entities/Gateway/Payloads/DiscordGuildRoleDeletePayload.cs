using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a guild role is deleted.
    /// </summary>
    [DiscordGatewayPayload("GUILD_ROLE_DELETE")]
    public sealed record DiscordGuildRoleDeletePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The role deleted.
        /// </summary>
        [JsonPropertyName("role_id")]
        public DiscordSnowflake RoleId { get; init; } = null!;
    }
}
