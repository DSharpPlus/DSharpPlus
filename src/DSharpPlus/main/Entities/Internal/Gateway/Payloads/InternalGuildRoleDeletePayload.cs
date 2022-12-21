using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a guild role is deleted.
    /// </summary>
    [InternalGatewayPayload("GUILD_ROLE_DELETE")]
    public sealed record InternalGuildRoleDeletePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The role deleted.
        /// </summary>
        [JsonPropertyName("role_id")]
        public InternalSnowflake RoleId { get; init; } = null!;
    }
}
