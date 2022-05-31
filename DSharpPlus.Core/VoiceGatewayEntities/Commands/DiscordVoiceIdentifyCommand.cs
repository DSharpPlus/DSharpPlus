using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

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
        [JsonProperty("server_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ServerId { get; init; } = null!;

        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake UserId { get; init; } = null!;

        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; init; } = null!;

        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; init; } = null!;
    }
}
