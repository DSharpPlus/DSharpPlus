namespace DSharpPlus.EventArgs;
using DSharpPlus.AsyncEvents;

/// <summary>
/// Represents base class for raw socket message event arguments.
/// </summary>
public abstract class SocketMessageEventArgs : AsyncEventArgs
{ }

/// <summary>
/// Represents arguments for text message websocket event.
/// </summary>
public sealed class SocketTextMessageEventArgs : SocketMessageEventArgs
{
    /// <summary>
    /// Gets the received message string.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a new instance of text message event arguments.
    /// </summary>
    /// <param name="message">Received message string.</param>
    public SocketTextMessageEventArgs(string message) => Message = message;
}

/// <summary>
/// Represents arguments for binary message websocket event.
/// </summary>
public sealed class SocketBinaryMessageEventArgs : SocketMessageEventArgs
{
    /// <summary>
    /// Gets the received message bytes.
    /// </summary>
    public byte[] Message { get; }

    /// <summary>
    /// Creates a new instance of binary message event arguments.
    /// </summary>
    /// <param name="message">Received message bytes.</param>
    public SocketBinaryMessageEventArgs(byte[] message) => Message = message;
}
