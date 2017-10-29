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
        /// <para>Sets the type of the token used to identify the client.</para>
        /// <para>Defaults to <see cref="TokenType.Bot"/>.</para>
        /// </summary>
        public TokenType TokenType { internal get; set; } = TokenType.Bot;

        /// <summary>
        /// <para>Sets the maximum logging level for messages.</para>
        /// <para>Typically, the default value of <see cref="LogLevel.Info"/> is ok for most uses.</para>
        /// </summary>
        public LogLevel LogLevel { internal get; set; } = LogLevel.Info;

        /// <summary>
        /// <para>Sets whether to use the internal log handler.</para>
        /// <para>This is disabled by default. Use it if you don't want to provide your own log handlers.</para>
        /// </summary>
        public bool UseInternalLogHandler { internal get; set; } = false;

        /// <summary>
        /// <para>Allows you to overwrite the time format used by the internal debug logger.</para>
        /// <para>Only applicable when <see cref="UseInternalLogHandler"/> is set to true. Defaults to ISO 8601-like format.</para>
        /// </summary>
        public string DateTimeFormat { internal get; set; } = "yyyy-MM-dd HH:mm:ss zzz";

        /// <summary>
        /// <para>Sets the member count threshold at which guilds are considered large.</para>
        /// <para>Defaults to 250.</para>
        /// </summary>
        public int LargeThreshold { internal get; set; } = 250;

        /// <summary>
        /// <para>Sets whether to automatically reconnect in case a connection is lost.</para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool AutoReconnect { internal get; set; } = true;

        /// <summary>
        /// <para>Sets the ID of the shard to connect to.</para>
        /// <para>If not sharding, or sharding automatically, this value should be left with the default value of 0.</para>
        /// </summary>
        public int ShardId { internal get; set; } = 0;

        /// <summary>
        /// <para>Sets the total number of shards the bot is on. If not sharding, this value should be left with a default value of 1.</para>
        /// <para>If sharding automatically, this value will indicate how many shards to boot. If left default for automatic sharding, the client will determine the shard count automatically.</para>
        /// </summary>
        public int ShardCount { internal get; set; } = 1;

        /// <summary>
        /// <para>Sets the level of compression for WebSocket traffic.</para>
        /// <para>Disabling this option will increase the amount of traffic sent via WebSocket. Setting <see cref="GatewayCompressionLevel.Payload"/> will enable compression for READY and GUILD_CREATE payloads. Setting <see cref="GatewayCompressionLevel.Stream"/> will enable compression for the entire WebSocket stream, drastically reducing amount of traffic.</para>
        /// <para>Defaults to <see cref="GatewayCompressionLevel.Stream"/>.</para>
        /// </summary>
        public GatewayCompressionLevel GatewayCompressionLevel { internal get; set; } = GatewayCompressionLevel.Stream;

        /// <summary>
        /// <para>Sets the size of the global message cache.</para>
        /// <para>Setting this to 0 will disable message caching entirely. Defaults to 1024.</para>
        /// </summary>
        public int MessageCacheSize { internal get; set; } = 1024;

        /// <summary>
        /// <para>Sets whether guilds should be automatically synced when logging in with a user token.</para>
        /// <para>Defaults to true.</para>
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
            this.DateTimeFormat = other.DateTimeFormat;
            this.LargeThreshold = other.LargeThreshold;
            this.AutoReconnect = other.AutoReconnect;
            this.ShardId = other.ShardId;
            this.ShardCount = other.ShardCount;
            this.GatewayCompressionLevel = other.GatewayCompressionLevel;
            this.MessageCacheSize = other.MessageCacheSize;
            this.AutomaticGuildSync = other.AutomaticGuildSync;
        }
    }
}
