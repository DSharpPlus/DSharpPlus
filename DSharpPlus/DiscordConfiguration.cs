using System;

namespace DSharpPlus
{
    /// <summary>
    /// Represents configuration for <see cref="DiscordClient"/> and <see cref="DiscordShardedClient"/>.
    /// </summary>
    public sealed class DiscordConfiguration
    {
        /// <summary>
        /// Sets the token used to identify the client.
        /// </summary>
        public string Token
        {
            internal get => this._token;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value), "Token cannot be null, empty, or all whitespace.");

                this._token = value.Trim();
            }
        }
        private string _token = "";

        /// <summary>
        /// Sets the type of the token used to identify the client. Defaults to <see cref="TokenType.Bot"/>.
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
        /// Allows you to overwrite the time format used by the internal debug logger. Only applicable when <see cref="UseInternalLogHandler"/> is set to true. Defaults to ISO 8601-like format.
        /// </summary>
        public string DateTimeFormat { internal get; set; } = "yyyy-MM-dd HH:mm:ss zzz";

        /// <summary>
        /// Sets the member count threshold at which guilds are considered large. Defaults to 250.
        /// </summary>
        public int LargeThreshold { internal get; set; } = 250;

        /// <summary>
        /// Sets whether to automatically reconnect in case a connection is lost. Defaults to true.
        /// </summary>
        public bool AutoReconnect { internal get; set; } = true;

        /// <summary>
        /// Sets the ID of the shard to connect to. If not sharding, or sharding automatically, this value should be left with the default value of 0.
        /// </summary>
        public int ShardId { internal get; set; } = 0;

        /// <summary>
        /// Sets the total number of shards the bot is on. If not sharding, this value should be left with a default value of 1. If sharding automatically, this value will indicate how many shards 
        /// to boot. If left default for automatic sharding, the client will determine the shard count automatically.
        /// </summary>
        public int ShardCount { internal get; set; } = 1;

        /// <summary>
        /// Sets whether to enable compression for gateway communication. Disabling this option will increase size of certain dispatches, and might increase login time. Defaults to true.
        /// </summary>
        public bool EnableCompression { internal get; set; } = true;

        /// <summary>
        /// Sets the size of the global message cache. Setting this to 0 will disable message caching entirely. Defaults to 1024.
        /// </summary>
        public int MessageCacheSize { internal get; set; } = 1024;

        /// <summary>
        /// Sets whether guilds should be automatically synced when logging in with a user token. Defaults to true.
        /// </summary>
        public bool AutomaticGuildSync { internal get; set; } = true;

        /// <summary>
        /// Creates a new configuration with default values.
        /// </summary>
        public DiscordConfiguration() { }

        /// <summary>
        /// Creates a clone of another discord configuration.
        /// </summary>
        /// <param name="other">Client configuration to clone.</param>
        public DiscordConfiguration(DiscordConfiguration other)
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
