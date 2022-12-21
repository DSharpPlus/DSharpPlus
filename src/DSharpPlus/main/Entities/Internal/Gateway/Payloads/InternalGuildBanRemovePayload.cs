using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user is unbanned from a guild.
    /// </summary>
    [InternalGatewayPayload("GUILD_BAN_REMOVE")]
    public sealed record InternalGuildBanRemovePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The unbanned user.
        /// </summary>
        [JsonPropertyName("user")]
        public InternalUser User { get; init; } = null!;
    }
}
