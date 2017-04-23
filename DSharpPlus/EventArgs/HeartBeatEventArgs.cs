using System;

namespace DSharpPlus
{
    public class HeartBeatEventArgs : DiscordEventArgs
    {
        public int Ping { get; internal set; }
        public DateTimeOffset Timestamp { get; internal set; }
        public int IntegrityChecksum { get; internal set; }

        public HeartBeatEventArgs(DiscordClient client) : base(client) { }
    }
}
