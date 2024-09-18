using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.ClientsConnected"/>
/// </summary>
internal sealed record VoiceClientsConnectedPayload : IVoicePayload
{
    /// <summary>
    /// The snowflake identifiers of users that became visible to the present client.
    /// This is fired when the bot connects to a voice channel, containing the IDs of
    /// all users already present in the channel.
    /// </summary>
    [JsonPropertyName("user_ids")]
    public required IReadOnlyList<ulong> UserIds { get; init; }
}
