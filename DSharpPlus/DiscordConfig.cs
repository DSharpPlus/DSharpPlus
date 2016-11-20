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
        public VoiceSettings VoiceSettings { get; set; } = VoiceSettings.None;
        public VoiceApplication VoiceApplication { get; set; } = VoiceApplication.Music;
    }
}
