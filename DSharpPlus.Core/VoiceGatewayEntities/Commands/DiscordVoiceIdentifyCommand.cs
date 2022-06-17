using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.VoiceGatewayEntities.Commands
{
    /// <summary>
    /// Once connected to the voice WebSocket endpoint, we can send an <see cref="Enums.DiscordVoiceOpCode.Identify"/> payload.
    /// </summary>
    public sealed record DiscordVoiceIdentifyCommand
    {
        /// <summary>
        /// Also known as the guild id.
        /// </summary>
        [JsonPropertyName("server_id")]
        public DiscordSnowflake ServerId { get; init; } = null!;

        [JsonPropertyName("user_id")]
        public DiscordSnowflake UserId { get; init; } = null!;

        [JsonPropertyName("session_id")]
        public string SessionId { get; init; } = null!;

        [JsonPropertyName("token")]
        public string Token { get; init; } = null!;
    }
}
