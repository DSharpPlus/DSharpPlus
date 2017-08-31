namespace DSharpPlus
{
    /// <summary>
    /// Represents configuration for <see cref="DiscordClient"/> and <see cref="DiscordShardedClient"/>.
    /// </summary>
    public sealed class DiscordConfig
    {
        /// <summary>
        /// Sets the token used to identify the client.
        /// </summary>
        public string Token
        {
            internal get => this._token;

            set => this._token = value.Trim();
        }
        private string _token = "";

        /// <summary>
        /// Sets the type of the token used to identify the client. This is typically <see cref="TokenType.Bot"/> for bots, and <see cref="TokenType.User"/> for selfbots and userbots.
        /// </summary>
        public TokenType TokenType { internal get; set; } = TokenType.Bot;

        /// <summary>
        /// Sets the maximum logging level for messages. Typically, the default value of <see cref="LogLevel.Info"/> is ok for most uses.
        /// </summary>
        public LogLevel LogLevel { internal get; set; } = LogLevel.Info;

        /// <summary>
        /// Sets whether to use the internal log handler. This is disabled by default. Use it if you don't want to provide your own log handlers.
        /// </summary>
        public bool UseInternalLogHandler { internal get; set; } = false;

        /// <summary>
        /// Sets the member count threshold at which guilds are considered large.
        /// </summary>
        public int LargeThreshold { internal get; set; } = 250;

        /// <summary>
        /// Sets whether to automatically reconnect in case a connection is lost.
        /// </summary>
        public bool AutoReconnect { internal get; set; } = true;

        /// <summary>
        /// Sets the ID of the shard to connect to. If not sharding, or sharding automatically, this value should be left default.
        /// </summary>
        public int ShardId { internal get; set; } = 0;

        /// <summary>
        /// Sets the total number of shards the bot is on. If not sharding, this value should be left default. If sharding automatically, this value will indicate how many shards to boot. If left default for automatic sharding, the client will determine the shard count automatically.
        /// </summary>
        public int ShardCount { internal get; set; } = 1;

        /// <summary>
        /// Sets whether to enable compression for gateway communication. Disabling this option will increase size of certain dispatches, and might increase login time.
        /// </summary>
        public bool EnableCompression { internal get; set; } = true;

        /// <summary>
        /// Sets the size of the per-channel message cache. Setting this to 0 will disable message caching completely. Do note that large cache will increase RAM usage drastically.
        /// </summary>
        public int MessageCacheSize { internal get; set; } = 50;

        /// <summary>
        /// Sets whether guilds should be automatically synced for user tokens.
        /// </summary>
        public bool AutomaticGuildSync { internal get; set; } = true;

        /// <summary>
        /// Creates a new configuration with default values.
        /// </summary>
        public DiscordConfig() { }

        /// <summary>
        /// Creates a clone of another discord configuration.
        /// </summary>
        /// <param name="other">Client configuration to clone.</param>
        public DiscordConfig(DiscordConfig other)
        {
            this.Token = other.Token;
            this.TokenType = other.TokenType;
            this.LogLevel = other.LogLevel;
            this.UseInternalLogHandler = other.UseInternalLogHandler;
            this.LargeThreshold = other.LargeThreshold;
            this.AutoReconnect = other.AutoReconnect;
            this.ShardId = other.ShardId;
            this.ShardCount = other.ShardCount;
            this.EnableCompression = other.EnableCompression;
            this.MessageCacheSize = other.MessageCacheSize;
        }
    }
}
