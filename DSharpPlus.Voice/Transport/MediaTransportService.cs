using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.Transport;

/// <inheritdoc/>
public class MediaTransportService : IMediaTransportService, IDisposable
{
    private readonly UdpClient udpClient;
    private readonly SemaphoreSlim writeSemaphore = new(1);
    private readonly SemaphoreSlim readSemaphore = new(1);

    /// <summary>
    /// Used by MediaTransportFactory to build the MediaTransportService.
    /// </summary>
    /// <param name="localBinding">Indicates where on the local machine UDP datagrams are bound to.</param> 
    /// <param name="remoteBinding">Indicates what remote address UDP datagrams are sent to.</param> 
    public MediaTransportService(IPEndPoint? localBinding, IPEndPoint? remoteBinding)
    {
        this.udpClient = localBinding is not null ? new UdpClient(localBinding) : new UdpClient();

        if (remoteBinding is not null)
        {
            this.udpClient.Connect(remoteBinding);
        }
    }

    /// <inheritdoc/>
    public async Task ReceiveAsync(IBufferWriter<byte> bufferWriter)
    {
        await this.readSemaphore.WaitAsync();

        try
        {
            UdpReceiveResult result = await this.udpClient.ReceiveAsync();
            Memory<byte> dst = bufferWriter.GetMemory(result.Buffer.Length);
            result.Buffer.AsSpan().CopyTo(dst.Span);
            bufferWriter.Advance(result.Buffer.Length);
        }
        finally
        {
            this.readSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> buffer)
    {
        await this.writeSemaphore.WaitAsync();
        try
        {
            await this.udpClient.SendAsync(buffer);
        }
        finally
        {
            this.writeSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose() => this.udpClient?.Dispose();
}
