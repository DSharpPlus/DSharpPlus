using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.Resume"/>.
/// </summary>
internal sealed record VoiceResumePayload : IVoicePayload
{
    /// <summary>
    /// The snowflake identifier of the guild the bot was connected to.
    /// </summary>
    [JsonPropertyName("server_id")]
    public required ulong ServerId { get; init; }

    /// <summary>
    /// The identifier of the session the bot is attempting to resume.
    /// </summary>
    [JsonPropertyName("session_id")]
    public required string SessionId { get; init; }

    /// <summary>
    /// The token of the current application.
    /// </summary>
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    /// <summary>
    /// The last sequence number acknowledged by the bpt-
    /// </summary>
    [JsonPropertyName("seq_ack")]
    public required uint LastAcknowledgedSequenceNumber { get; init; }
}
