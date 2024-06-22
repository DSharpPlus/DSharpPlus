using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents the lowest-level transport layer over the gateway. This type is not thread-safe.
/// </summary>
public interface ITransportService : IDisposable
{
    /// <summary>
    /// Opens a connection to the gateway.
    /// </summary>
    public ValueTask ConnectAsync(string url);

    /// <summary>
    /// Reads the next message from the gateway asynchronously.
    /// </summary>
    public ValueTask<TransportFrame> ReadAsync();

    /// <summary>
    /// Writes the specified message to the gateway.
    /// </summary>
    public ValueTask WriteAsync(byte[] payload);

    /// <summary>
    /// Disconnects from the gateway.
    /// </summary>
    /// <param name="closeStatus">The status message to send to the Discord gateway.</param>
    public ValueTask DisconnectAsync(WebSocketCloseStatus closeStatus);
}
