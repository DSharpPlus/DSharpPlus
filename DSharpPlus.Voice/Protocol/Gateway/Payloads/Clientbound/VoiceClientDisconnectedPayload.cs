using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.ClientDisconnected"/>
/// </summary>
internal sealed record VoiceClientDisconnectedPayload : IVoicePayload
{
    /// <summary>
    /// The snowflake identifier of the user who disconnected.
    /// </summary>
    [JsonPropertyName("user_id")]
    [JsonConverter(typeof(SnowflakeConverter))]
    public ulong UserId { get; init; }
}
