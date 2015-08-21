namespace DiscordSharp
{
    public enum DiscordUserStatus { ONLINE, IDLE, OFFLINE }

    public class DiscordPresenceUpdateEventArgs
    {
        public DiscordMember user { get; internal set; }
        public DiscordUserStatus status { get; internal set; }
        public string game_id { get; internal set; }
    }
}