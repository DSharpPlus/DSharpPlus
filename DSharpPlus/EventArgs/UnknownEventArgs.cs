namespace DSharpPlus
{
    public class UnknownEventArgs : DiscordEventArgs
    {
        public string EventName { get; internal set; }
        public string Json { get; internal set; }

        public UnknownEventArgs(DiscordClient client) : base(client) { }
    }
}
