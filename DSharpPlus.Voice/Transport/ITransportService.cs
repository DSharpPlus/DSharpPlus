using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

using DSharpPlus.Voice.Protocol.Gateway;

namespace DSharpPlus.Voice.Transport;

/// <summary>
/// Represents a mechanism for transporting data to and from the voice gateway server.
/// </summary>
public interface ITransportService : IDisposable
{
    /// <summary>
    /// Current sequence number for the active connection
    /// </summary>
    public ushort SequenceNumber { get; }

    /// <summary>
    /// Opens a connection to the voice gateway.
    /// </summary>
    public Task ConnectAsync(string url, ulong channelId);

    /// <summary>
    /// Sends the specified payload as binary data.
    /// </summary>
    public Task SendBinaryAsync(ReadOnlyMemory<byte> payload);

    /// <summary>
    /// Sends the specified payload as JSON.
    /// </summary>
    public Task SendTextAsync(VoiceGatewayMessage payload);

    /// <summary>
    /// Receives a frame from the voice gateway.
    /// </summary>
    public Task<VoiceGatewayTransportFrame> ReceiveAsync();

    /// <summary>
    /// Disconnects from the gateway.
    /// </summary>
    /// <param name="status">The status message to send to the voice gateway.</param>
    public Task DisconnectAsync(WebSocketCloseStatus status);
}
