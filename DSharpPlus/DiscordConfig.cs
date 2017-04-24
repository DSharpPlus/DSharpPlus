namespace DSharpPlus
{
    public class DiscordConfig
    {
        public Branch DiscordBranch { get; set; } = Branch.Stable;
        public string Token { get; set; } = "";
        public TokenType TokenType { get; set; } = TokenType.Bot;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public bool UseInternalLogHandler { get; set; } = false;
        public int LargeThreshold { get; set; } = 50;
        public bool AutoReconnect { get; set; } = false;
        public int ShardId { get; set; } = 0;
        public int ShardCount { get; set; } = 1;

        public DiscordConfig() { }
        public DiscordConfig(DiscordConfig other)
        {
            this.DiscordBranch = other.DiscordBranch;
            this.Token = other.Token;
            this.TokenType = other.TokenType;
            this.LogLevel = other.LogLevel;
            this.UseInternalLogHandler = other.UseInternalLogHandler;
            this.LargeThreshold = other.LargeThreshold;
            this.AutoReconnect = other.AutoReconnect;
            this.ShardId = other.ShardId;
            this.ShardCount = other.ShardCount;
        }
    }
}
