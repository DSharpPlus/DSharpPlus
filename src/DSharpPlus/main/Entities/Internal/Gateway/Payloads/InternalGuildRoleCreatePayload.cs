using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a guild role is created.
    /// </summary>
    [InternalGatewayPayload("GUILD_ROLE_CREATE")]
    public sealed record InternalGuildRoleCreatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The role created.
        /// </summary>
        [JsonPropertyName("role")]
        public InternalRole Role { get; init; } = null!;
    }
}
