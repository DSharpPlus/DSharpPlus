using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// The ready event is dispatched when a client has completed the initial handshake with the gateway (for new sessions). The ready event can be the largest and most complex event the gateway will send, as it contains all the state required for a client to begin interacting with the rest of the platform.
    /// </summary>
    [DiscordGatewayPayload("READY")]
    public sealed record DiscordReadyPayload
    {
        /// <summary>
        /// The gateway version.
        /// </summary>
        [JsonPropertyName("v")]
        public int Version { get; init; }

        /// <summary>
        /// Information about the user including email.
        /// </summary>
        [JsonPropertyName("user")]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The guilds the user is in.
        /// </summary>
        [JsonPropertyName("guilds")]
        public IReadOnlyList<DiscordGuild> Guilds { get; init; } = Array.Empty<DiscordGuild>();

        /// <summary>
        /// Used for resuming connections.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; init; } = null!;

        /// <summary>
        /// The shard information associated with this session, if sent when identifying.
        /// </summary>
        [JsonPropertyName("shard")]
        public Optional<IReadOnlyList<int>> Shard { get; init; }

        /// <summary>
        /// Contains <see cref="DiscordApplication.Id"> and <see cref="DiscordApplication.Flags"/>
        /// </summary>
        [JsonPropertyName("application")]
        public DiscordApplication Application { get; init; } = null!;
    }
}
