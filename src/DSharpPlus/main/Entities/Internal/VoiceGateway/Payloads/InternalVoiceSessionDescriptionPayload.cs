using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.VoiceGateway.Payloads;

/// <summary>
/// Finally, the voice server will respond with a <see cref="Enums.InternalVoiceOpCode.SessionDescription"/> that includes the <c>mode</c> and <c>secret_key</c>, a 32 byte array used for encrypting and sending voice data:
/// </summary>
public sealed record InternalVoiceSessionDescriptionPayload
{
    [JsonPropertyName("mode")]
    public string Mode { get; init; } = null!;

    [JsonPropertyName("secret_key")]
    public IReadOnlyList<byte> SecretKey { get; init; } = Array.Empty<byte>();
}
