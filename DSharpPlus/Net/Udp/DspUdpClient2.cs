using System;
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
            => throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");

        /// <summary>
        /// Creates a new UDP client instance.
        /// </summary>
        public DspUdpClient()
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Configures the UDP client.
        /// </summary>
        /// <param name="endpoint">Endpoint that the client will be communicating with.</param>
        public override void Setup(ConnectionEndpoint endpoint)
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Sends a datagram.
        /// </summary>
        /// <param name="data">Datagram.</param>
        /// <param name="data_length">Length of the datagram.</param>
        /// <returns></returns>
        public override Task SendAsync(byte[] data, int data_length)
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Receives a datagram.
        /// </summary>
        /// <returns>The received bytes.</returns>
        public override Task<byte[]> ReceiveAsync()
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Closes and disposes the client.
        /// </summary>
        public override void Close()
        {
            throw new PlatformNotSupportedException(".NET UDP client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Creates a new instance of <see cref="DspUdpClient"/>.
        /// </summary>
        /// <returns>An instance of <see cref="DspUdpClient"/>.</returns>
        public static BaseUdpClient CreateNew()
            => new DspUdpClient();
    }
}
