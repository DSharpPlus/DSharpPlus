using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Lavalink.Entities;

namespace DSharpPlus.Lavalink.EventArgs
{
    public sealed class StatsReceivedEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the Lavalink statistics received.
        /// </summary>
        public LavalinkStatistics Statistics { get; }


        public StatsReceivedEventArgs(LavalinkStatistics stats)
        {
            this.Statistics = stats;
        }
    }
}
