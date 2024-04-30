namespace DSharpPlus.Lavalink.EventArgs;

using DSharpPlus.AsyncEvents;
using DSharpPlus.Lavalink.Entities;

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
