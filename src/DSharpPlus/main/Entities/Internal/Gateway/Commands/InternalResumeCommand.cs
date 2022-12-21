using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Commands
{
    /// <summary>
    /// Used to replay missed events when a disconnected client resumes.
    /// </summary>
    public sealed record InternalResumeCommand
    {
        /// <summary>
        /// The session token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The session id.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; init; } = null!;

        /// <summary>
        /// The last sequence number received.
        /// </summary>
        [JsonPropertyName("seq")]
        public int Sequence { get; init; }
    }
}
