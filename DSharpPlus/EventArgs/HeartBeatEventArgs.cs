﻿using System;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.Heartbeated"/> event.
    /// </summary>
    public class HeartbeatEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the round-trip time of the heartbeat.
        /// </summary>
        public int Ping { get; internal set; }

        /// <summary>
        /// Gets the timestamp of the heartbeat.
        /// </summary>
        public DateTimeOffset Timestamp { get; internal set; }

        internal HeartbeatEventArgs(DiscordClient client) : base(client) { }
    }
}
