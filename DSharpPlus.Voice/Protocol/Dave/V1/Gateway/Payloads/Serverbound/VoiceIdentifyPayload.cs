using System.Text.Json.Serialization;

using DSharpPlus.Entities;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.Identify"/>
/// </summary>
internal sealed record VoiceIdentifyPayload : IVoicePayload
{
    /// <summary>
    /// The snowflake identifier of the guild the voice channel lies in, if applicable.
    /// </summary>
    [JsonPropertyName("server_id")]
    public Optional<ulong> GuildId { get; init; }

    /// <summary>
    /// The snowflake identifier of the current user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public required ulong UserId { get; init; }

    /// <summary>
    /// An identifier for this voice session.
    /// </summary>
    [JsonPropertyName("session_id")]
    public required string SessionId { get; init; }

    /// <summary>
    /// The token for this connection.
    /// </summary>
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    /// <summary>
    /// The highest supported DAVE protocol version. This must always be <c>1</c>.
    /// </summary>
    [JsonPropertyName("max_dave_protocol_version")]
    public required int HighestSupportedProtocolVersion { get; init; }
}
