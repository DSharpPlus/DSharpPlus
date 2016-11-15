namespace DSharpPlus
{
    public class DiscordWebhook : SnowflakeObject
    {
        public ulong GuildID { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordUser User { get; internal set; }
        public string Name { get; internal set; }
        public string Avatar { get; internal set; }
        public string Token { get; internal set; }
    }
}
