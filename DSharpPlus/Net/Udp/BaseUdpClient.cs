using System;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Udp
{
    public abstract class BaseUdpClient
    {
        /// <summary>
        /// Gets the amount of data available for this client.
        /// </summary>
        public abstract int DataAvailable { get; }

        internal static Type ClientType { get; set; } = typeof(DspUdpClient);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns></returns>
        public static BaseUdpClient Create()
        {
            return (BaseUdpClient)Activator.CreateInstance(ClientType);
        }

        /// <summary>
        /// Configures the UDP client.
        /// </summary>
        /// <param name="endpoint">Endpoint that the client will be communicating with.</param>
        public abstract void Setup(ConnectionEndpoint endpoint);

        /// <summary>
        /// Sends a datagram.
        /// </summary>
        /// <param name="data">Datagram.</param>
        /// <param name="data_length">Length of the datagram.</param>
        /// <returns></returns>
        public abstract Task SendAsync(byte[] data, int data_length);

        /// <summary>
        /// Receives a datagram.
        /// </summary>
        /// <returns></returns>
        public abstract Task<byte[]> ReceiveAsync();

        /// <summary>
        /// Closes and disposes the client.
        /// </summary>
        public abstract void Close();
    }
}
