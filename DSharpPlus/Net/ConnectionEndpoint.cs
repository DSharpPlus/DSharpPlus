namespace DSharpPlus.Net
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

        /// <summary>
        /// Creates a new endpoint structure.
        /// </summary>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="port">Port to use for connection.</param>
        public ConnectionEndpoint(string hostname, int port)
        {
            this.Hostname = hostname;
            this.Port = port;
        }

        /// <summary>
        /// Gets the hash code of this endpoint.
        /// </summary>
        /// <returns>Hash code of this endpoint.</returns>
        public override int GetHashCode()
        {
            return 13 + 7 * this.Hostname.GetHashCode() + 7 * this.Port;
        }

        /// <summary>
        /// Gets the string representation of this connection endpoint.
        /// </summary>
        /// <returns>String representation of this endpoint.</returns>
        public override string ToString()
        {
            return $"{this.Hostname}:{this.Port}";
        }
    }
}
