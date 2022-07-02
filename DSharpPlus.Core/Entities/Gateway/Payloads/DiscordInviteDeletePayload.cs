using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when an invite is deleted.
    /// </summary>
    [DiscordGatewayPayload("INVITE_DELETE")]
    public sealed record DiscordInviteDeletePayload
    {
        /// <summary>
        /// The channel of the invite.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The guild of the invite.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The unique invite code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; init; } = null!;
    }
}
