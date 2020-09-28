﻿using System;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.ChannelPinsUpdated"/> event.
    /// </summary>
    public class ChannelPinsUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild in which the update occurred.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the channel in which the update occurred.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the timestamp of the latest pin.
        /// </summary>
        public DateTimeOffset? LastPinTimestamp { get; internal set; }

        internal ChannelPinsUpdateEventArgs() : base() { }
    }
}
