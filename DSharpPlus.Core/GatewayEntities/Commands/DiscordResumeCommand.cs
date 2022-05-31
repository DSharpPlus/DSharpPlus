using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Commands
{
    /// <summary>
    /// Used to replay missed events when a disconnected client resumes.
    /// </summary>
    public sealed record DiscordResumeCommand
    {
        /// <summary>
        /// The session token.
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The session id.
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; init; } = null!;

        /// <summary>
        /// The last sequence number received.
        /// </summary>
        [JsonProperty("seq", NullValueHandling = NullValueHandling.Ignore)]
        public int Sequence { get; init; }
    }
}
