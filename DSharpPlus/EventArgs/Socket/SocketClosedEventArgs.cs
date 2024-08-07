namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for SocketClosed event.
/// </summary>
public class SocketClosedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the close code sent by remote host.
    /// </summary>
    public int CloseCode { get; internal set; }

    /// <summary>
    /// Gets the close message sent by remote host.
    /// </summary>
    public string CloseMessage { get; internal set; }

    internal SocketClosedEventArgs() : base() { }
}
