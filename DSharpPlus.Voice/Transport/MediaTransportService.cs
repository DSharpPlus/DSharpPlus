using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;

/// <inheritdoc/>
public class MediaTransportService : IMediaTransportService
{
    private readonly UdpClient udpClient;

    public MediaTransportService() 
        => this.udpClient = new();

    /// <inheritdoc/>
    public Task ConnectAsync(IPEndPoint endpoint)
    {
        this.udpClient.Connect(endpoint);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task ReceiveAsync(IBufferWriter<byte> bufferWriter)
    {
        UdpReceiveResult result = await this.udpClient.ReceiveAsync();
        Memory<byte> dst = bufferWriter.GetMemory(result.Buffer.Length);
        result.Buffer.AsSpan().CopyTo(dst.Span);
        bufferWriter.Advance(result.Buffer.Length);
    }

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> buffer) 
        => await this.udpClient.SendAsync(buffer);

    /// <inheritdoc/>
    public Task DisconnectAsync()
    {
        this.udpClient.Close();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose() 
        => this.udpClient.Dispose();
}
