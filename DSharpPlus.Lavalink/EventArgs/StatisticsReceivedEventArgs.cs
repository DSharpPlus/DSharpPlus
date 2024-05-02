
using DSharpPlus.AsyncEvents;
using DSharpPlus.Lavalink.Entities;

namespace DSharpPlus.Lavalink.EventArgs;
/// <summary>
/// Represents arguments for Lavalink statistics received.
/// </summary>
public sealed class StatisticsReceivedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the Lavalink statistics received.
    /// </summary>
    public LavalinkStatistics Statistics { get; }

    internal StatisticsReceivedEventArgs(LavalinkStatistics stats) => Statistics = stats;
}
