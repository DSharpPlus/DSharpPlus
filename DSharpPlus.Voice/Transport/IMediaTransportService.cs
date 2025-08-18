using System;
using System.Buffers;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;

/// <summary>
/// Provides a mechanism to send and receive data across UDP.
/// </summary>
public interface IMediaTransportService
{
    /// <summary>
    /// Sends the buffer to the configured remote endpoint as a datagram.
    /// </summary>
    public Task SendAsync(ReadOnlyMemory<byte> buffer);

    /// <summary>
    /// Waits for a UDP datagram and writes it to the provided <paramref name="bufferWriter"/>.
    /// </summary>
    public Task ReceiveAsync(IBufferWriter<byte> bufferWriter);
}
