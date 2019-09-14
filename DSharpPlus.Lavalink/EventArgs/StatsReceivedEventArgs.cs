using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Lavalink.Entities;

namespace DSharpPlus.Lavalink.EventArgs
{
    /// <summary>
    /// Represents arguments for Lavalink statistics received.
    /// </summary>
    public sealed class StatsReceivedEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the Lavalink statistics received.
        /// </summary>
        public LavalinkStatistics Statistics { get; }


        internal StatsReceivedEventArgs(LavalinkStatistics stats)
        {
            this.Statistics = stats;
        }
    }
}
