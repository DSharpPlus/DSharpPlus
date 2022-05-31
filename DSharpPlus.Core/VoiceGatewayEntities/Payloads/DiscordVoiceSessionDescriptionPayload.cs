using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.VoiceGatewayEntities.Payloads
{
    /// <summary>
    /// Finally, the voice server will respond with a <see cref="Enums.DiscordVoiceOpCode.SessionDescription"/> that includes the <c>mode</c> and <c>secret_key</c>, a 32 byte array used for encrypting and sending voice data:
    /// </summary>
    public sealed record DiscordVoiceSessionDescriptionPayload
    {
        [JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
        public string Mode { get; init; } = null!;

        [JsonProperty("secret_key", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<byte> SecretKey { get; init; } = Array.Empty<byte>();
    }
}
