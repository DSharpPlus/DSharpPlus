namespace DSharpPlus.Net.Udp;

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The default, native-based UDP client implementation.
/// </summary>
internal class DspUdpClient : BaseUdpClient
{
    private UdpClient Client { get; set; }
    private ConnectionEndpoint EndPoint { get; set; }
    private BlockingCollection<byte[]> PacketQueue { get; }

    private Task ReceiverTask { get; set; }
    private CancellationTokenSource TokenSource { get; }
    private CancellationToken Token => TokenSource.Token;

    /// <summary>
    /// Creates a new UDP client instance.
    /// </summary>
    public DspUdpClient()
    {
        PacketQueue = new BlockingCollection<byte[]>();
        TokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Configures the UDP client.
    /// </summary>
    /// <param name="endpoint">Endpoint that the client will be communicating with.</param>
    public override void Setup(ConnectionEndpoint endpoint)
    {
        EndPoint = endpoint;
        Client = new UdpClient();
        ReceiverTask = Task.Run(ReceiverLoopAsync, Token);
    }

    /// <summary>
    /// Sends a datagram.
    /// </summary>
    /// <param name="data">Datagram.</param>
    /// <param name="dataLength">Length of the datagram.</param>
    /// <returns></returns>
    public override Task SendAsync(byte[] data, int dataLength)
        => Client.SendAsync(data, dataLength, EndPoint.Hostname, EndPoint.Port);

    /// <summary>
    /// Receives a datagram.
    /// </summary>
    /// <returns>The received bytes.</returns>
    public override Task<byte[]> ReceiveAsync() => Task.FromResult(PacketQueue.Take(Token));

    /// <summary>
    /// Closes and disposes the client.
    /// </summary>
    public override void Close()
    {
        TokenSource.Cancel();
#if !NETSTANDARD1_3
        try
        { Client.Close(); }
        catch (Exception) { }
#endif

        // dequeue all the packets
        PacketQueue.Dispose();
    }

    private async Task ReceiverLoopAsync()
    {
        while (!Token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult packet = await Client.ReceiveAsync();
                PacketQueue.Add(packet.Buffer);
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
