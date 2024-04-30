namespace DSharpPlus.EventArgs;

using System;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.SocketClosed"/> event.
/// </summary>
public class SocketCloseEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the close code sent by remote host.
    /// </summary>
    public int CloseCode { get; internal set; }

    /// <summary>
    /// Gets the close message sent by remote host.
    /// </summary>
    public string CloseMessage { get; internal set; }

    public SocketCloseEventArgs() : base() { }
}

/// <summary>
/// Represents arguments for <see cref="DiscordClient.SocketErrored"/> event.
/// </summary>
public class SocketErrorEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the exception thrown by websocket client.
    /// </summary>
    public Exception Exception { get; internal set; }

    public SocketErrorEventArgs() : base() { }
}
