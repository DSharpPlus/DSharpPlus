using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Udp;

/// <summary>
/// The default, native-based UDP client implementation.
/// </summary>
internal class DspUdpClient : BaseUdpClient
{
    private UdpClient Client { get; set; }
    private ConnectionEndpoint EndPoint { get; set; }
    private BlockingCollection<byte[]> PacketQueue { get; }

    private CancellationTokenSource TokenSource { get; }
    private CancellationToken Token => this.TokenSource.Token;

    /// <summary>
    /// Creates a new UDP client instance.
    /// </summary>
    public DspUdpClient()
    {
        this.PacketQueue = [];
        this.TokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Configures the UDP client.
    /// </summary>
    /// <param name="endpoint">Endpoint that the client will be communicating with.</param>
    public override void Setup(ConnectionEndpoint endpoint)
    {
        this.EndPoint = endpoint;
        this.Client = new UdpClient();
        _ = Task.Run(ReceiverLoopAsync, this.Token);
    }

    /// <summary>
    /// Sends a datagram.
    /// </summary>
    /// <param name="data">Datagram.</param>
    /// <param name="dataLength">Length of the datagram.</param>
    /// <returns></returns>
    public override Task SendAsync(byte[] data, int dataLength)
        => this.Client.SendAsync(data, dataLength, this.EndPoint.Hostname, this.EndPoint.Port);

    /// <summary>
    /// Receives a datagram.
    /// </summary>
    /// <returns>The received bytes.</returns>
    public override Task<byte[]> ReceiveAsync() => Task.FromResult(this.PacketQueue.Take(this.Token));

    /// <summary>
    /// Closes and disposes the client.
    /// </summary>
    public override void Close()
    {
        this.TokenSource.Cancel();
#if !NETSTANDARD1_3
        try
        { this.Client.Close(); }
        catch (Exception) { }
#endif

        // dequeue all the packets
        this.PacketQueue.Dispose();
    }

    private async Task ReceiverLoopAsync()
    {
        while (!this.Token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult packet = await this.Client.ReceiveAsync();
                this.PacketQueue.Add(packet.Buffer);
            }
            catch (Exception) { }
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="DspUdpClient"/>.
    /// </summary>
    /// <returns>An instance of <see cref="DspUdpClient"/>.</returns>
    public static BaseUdpClient CreateNew()
        => new DspUdpClient();
}
