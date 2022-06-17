using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a guild role is created.
    /// </summary>
    [DiscordGatewayPayload("GUILD_ROLE_CREATE")]
    public sealed record DiscordGuildRoleCreatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The role created.
        /// </summary>
        [JsonPropertyName("role")]
        public DiscordRole Role { get; init; } = null!;
    }
}
