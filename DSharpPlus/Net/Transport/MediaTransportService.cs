using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Transport;
public class MediaTransportService : IMediaTransportService, IDisposable
{
    private readonly UdpClient udpClient;
    private readonly SemaphoreSlim writeSemaphore = new(1);
    private readonly SemaphoreSlim readSemaphore = new(1);

    public MediaTransportService(IPEndPoint? localBinding, IPEndPoint? remoteBinding)
    {
        this.udpClient = localBinding is not null ? new UdpClient(localBinding) : new UdpClient();

        if (remoteBinding is not null)
        {
            this.udpClient.Connect(remoteBinding);
        }
    }

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

    public void Dispose() => this.udpClient?.Dispose();
}
