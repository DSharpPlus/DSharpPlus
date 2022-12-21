using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a guild role is updated.
    /// </summary>
    [InternalGatewayPayload("GUILD_ROLE_UPDATE")]
    public sealed record InternalGuildRoleUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The role updated.
        /// </summary>
        [JsonPropertyName("role")]
        public InternalRole Role { get; init; } = null!;
    }
}
