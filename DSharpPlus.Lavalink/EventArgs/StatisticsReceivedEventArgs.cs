using DSharpPlus.Lavalink.Entities;
using Emzi0767.Utilities;

namespace DSharpPlus.Lavalink.EventArgs
{
    /// <summary>
    /// Represents arguments for Lavalink statistics received.
    /// </summary>
    public sealed class StatisticsReceivedEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the Lavalink statistics received.
        /// </summary>
        public LavalinkStatistics Statistics { get; }


        internal StatisticsReceivedEventArgs(LavalinkStatistics stats)
        {
            this.Statistics = stats;
        }
    }
}
