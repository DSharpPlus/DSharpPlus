using System;
using System.Buffers;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Transport;
public interface IMediaTransportService
{
    /// <summary>
    /// Sends buffer to the configured remote endpoint as a datagram
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public Task SendAsync(ReadOnlyMemory<byte> buffer);
    /// <summary>
    /// Waits for a UDP datagram and on receit writes the datagram to bufferWriter
    /// </summary>
    /// <param name="bufferWriter"></param>
    /// <returns></returns>
    public Task ReceiveAsync(IBufferWriter<byte> bufferWriter);
}
