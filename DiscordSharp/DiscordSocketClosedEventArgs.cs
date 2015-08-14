namespace DiscordSharp
{
    public class DiscordSocketClosedEventArgs
    {
        public ushort Code { get; internal set; }
        public string Reason { get; internal set; }
        public bool WasClean { get; internal set; }

    }
}