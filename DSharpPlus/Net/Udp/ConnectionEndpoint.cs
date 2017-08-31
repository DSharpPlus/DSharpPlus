namespace DSharpPlus.Net.Udp
{
    /// <summary>
    /// Represents a network connection endpoint.
    /// </summary>
    public struct ConnectionEndpoint
    {
        /// <summary>
        /// Gets or sets the hostname associated with this endpoint.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the port associated with this endpoint.
        /// </summary>
        public int Port { get; set; }
    }
}
