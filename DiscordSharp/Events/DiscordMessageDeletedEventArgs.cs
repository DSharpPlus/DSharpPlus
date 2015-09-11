namespace DiscordSharp.Events
{
    public class DiscordMessageDeletedEventArgs
    {
        public DiscordMessage DeletedMessage { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
    }
}
