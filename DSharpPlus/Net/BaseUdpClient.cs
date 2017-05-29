using System;
using System.Net;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public abstract class BaseUdpClient
    {
        internal static Type ClientType { get; set; } = typeof(DspUdpClient);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns></returns>
        public static BaseUdpClient Create()
        {
            return (BaseUdpClient)Activator.CreateInstance(ClientType);
        }

        public abstract void Setup(ConnectionEndpoint endpoint);
        public abstract Task SendAsync(byte[] data, int data_length);
        public abstract Task<byte[]> ReceiveAsync();
        public abstract void Close();
    }
}
