using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user is banned from a guild.
    /// </summary>
    [InternalGatewayPayload("GUILD_BAN_ADD")]
    public sealed record InternalGuildBanAddPayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The banned user.
        /// </summary>
        [JsonPropertyName("user")]
        public InternalUser User { get; init; } = null!;
    }
}
