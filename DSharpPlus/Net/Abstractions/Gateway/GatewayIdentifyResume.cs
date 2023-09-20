using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Represents data for websocket identify payload.
    /// </summary>
    internal sealed class GatewayIdentify
    {
        /// <summary>
        /// Gets or sets the token used to identify the client to Discord.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the client's properties.
        /// </summary>
        [JsonProperty("properties")]
        public ClientProperties ClientProperties { get; } = new ClientProperties();

        /// <summary>
        /// Gets or sets whether to encrypt websocket traffic.
        /// </summary>
        [JsonProperty("compress")]
        public bool Compress { get; set; }

        /// <summary>
        /// Gets or sets the member count at which the guild is to be considered large.
        /// </summary>
        [JsonProperty("large_threshold")]
        public int LargeThreshold { get; set; }

        /// <summary>
        /// Gets or sets the shard info for this connection.
        /// </summary>
        [JsonProperty("shard")]
        public ShardInfo ShardInfo { get; set; }

        /// <summary>
        /// Gets or sets the presence for this connection.
        /// </summary>
		[JsonProperty("presence", NullValueHandling = NullValueHandling.Ignore)]
        public StatusUpdate Presence { get; set; } = null;

        /// <summary>
        /// Gets or sets the intent flags for this connection.
        /// </summary>
        [JsonProperty("intents")]
        public DiscordIntents Intents { get; set; }
    }

    /// <summary>
    /// Represents data for websocket identify payload.
    /// </summary>
    internal sealed class GatewayResume
    {
        /// <summary>
        /// Gets or sets the token used to identify the client to Discord.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the session id used to resume last session.
        /// </summary>
        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the last received sequence number.
        /// </summary>
        [JsonProperty("seq")]
        public long SequenceNumber { get; set; }
    }
}
