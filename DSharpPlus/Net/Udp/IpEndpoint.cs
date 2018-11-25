#if !NETSTANDARD1_1
using System.Net;

namespace DSharpPlus.Net.Udp
{
    /// <summary>
    /// Represents a network connection IP endpoint.
    /// </summary>
    public struct IpEndpoint
    {
        /// <summary>
        /// Gets or sets the hostname associated with this endpoint.
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        /// Gets or sets the port associated with this endpoint.
        /// </summary>
        public int Port { get; set; }
    }
}
#endif