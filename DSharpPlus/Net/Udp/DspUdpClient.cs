#if !WINDOWS_8
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Udp
{
    /// <summary>
    /// The default, native-based UDP client implementation.
    /// </summary>
    internal class DspUdpClient : BaseUdpClient
    {
        /// <summary>
        /// Gets the amount of data available for this client.
        /// </summary>
        public override int DataAvailable 
            => Client.Available;

        private UdpClient Client { get; set; }
        private ConnectionEndpoint EndPoint { get; set; }

        /// <summary>
        /// Creates a new UDP client instance.
        /// </summary>
        public DspUdpClient() { }

        /// <summary>
        /// Configures the UDP client.
        /// </summary>
        /// <param name="endpoint">Endpoint that the client will be communicating with.</param>
        public override void Setup(ConnectionEndpoint endpoint)
        {
            EndPoint = endpoint;
            Client = new UdpClient();
            // TODO: Solve for .NET Standard, this is possibly default behaviour (???)
#if HAS_NAT_TRAVERSAL
            Client.AllowNatTraversal(true);
#endif
        }

        /// <summary>
        /// Sends a datagram.
        /// </summary>
        /// <param name="data">Datagram.</param>
        /// <param name="data_length">Length of the datagram.</param>
        /// <returns></returns>
        public override Task SendAsync(byte[] data, int data_length)
            => Client.SendAsync(data, data_length, EndPoint.Hostname, EndPoint.Port);

        /// <summary>
        /// Receives a datagram.
        /// </summary>
        /// <returns>The received bytes.</returns>
        public override async Task<byte[]> ReceiveAsync()
        {
            var result = await Client.ReceiveAsync().ConfigureAwait(false);
            return result.Buffer;
        }

        /// <summary>
        /// Closes and disposes the client.
        /// </summary>
        public override void Close()
        {
            // TODO: Solve later
        }

        /// <summary>
        /// Creates a new instance of <see cref="DspUdpClient"/>.
        /// </summary>
        /// <returns>An instance of <see cref="DspUdpClient"/>.</returns>
        public static BaseUdpClient CreateNew()
            => new DspUdpClient();
    }
}
#endif