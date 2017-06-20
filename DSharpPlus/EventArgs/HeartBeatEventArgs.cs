using System;

namespace DSharpPlus
{
    public class HeartBeatEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// heartbeat ping
        /// </summary>
        public int Ping { get; internal set; }
        /// <summary>
        /// Heartbeat timestamp
        /// </summary>
        public DateTimeOffset Timestamp { get; internal set; }
        /// <summary>
        /// Checksum of heartbeat's integrity
        /// </summary>
        public int IntegrityChecksum { get; internal set; }

        public HeartBeatEventArgs(DiscordClient client) : base(client) { }
    }
}
