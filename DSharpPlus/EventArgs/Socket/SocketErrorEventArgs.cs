using System;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for SocketErrored event.
/// </summary>
public class SocketErrorEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the exception thrown by websocket client.
    /// </summary>
    public Exception Exception { get; internal set; }

    public SocketErrorEventArgs() : base() { }
}
