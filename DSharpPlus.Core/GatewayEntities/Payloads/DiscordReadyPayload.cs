using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
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
        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public int Version { get; init; }

        /// <summary>
        /// Information about the user including email.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The guilds the user is in.
        /// </summary>
        [JsonProperty("guilds", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordGuild> Guilds { get; init; } = Array.Empty<DiscordGuild>();

        /// <summary>
        /// Used for resuming connections.
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; init; } = null!;

        /// <summary>
        /// The shard information associated with this session, if sent when identifying.
        /// </summary>
        [JsonProperty("shard", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<int>> Shard { get; init; }

        /// <summary>
        /// Contains <see cref="DiscordApplication.Id"> and <see cref="DiscordApplication.Flags"/>
        /// </summary>
        [JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordApplication Application { get; init; } = null!;
    }
}
