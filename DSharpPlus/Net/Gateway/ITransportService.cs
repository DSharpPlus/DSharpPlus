using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

using DSharpPlus.Net.Gateway.Compression;

namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents the lowest-level transport layer over a websocket.
/// </summary>
public interface ITransportService : IDisposable
{
    /// <summary>
    /// Initializes the transport service.
    /// </summary>
    public void Initialize(string loggerName, IPayloadDecompressor decompressor);

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
    public ValueTask WriteAsync(ReadOnlyMemory<byte> payload, WebSocketMessageType messageType = WebSocketMessageType.Text);

    /// <summary>
    /// Disconnects from the gateway.
    /// </summary>
    /// <param name="closeStatus">The status message to send to the Discord gateway.</param>
    public ValueTask DisconnectAsync(WebSocketCloseStatus closeStatus);
}
