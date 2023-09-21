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
        /// Gets or sets the secured status of this connection.
        /// </summary>
        public bool Secured { get; set; }

        /// <summary>
        /// Creates a new endpoint structure.
        /// </summary>
        /// <param name="hostname">Hostname to connect to.</param>
        /// <param name="port">Port to use for connection.</param>
        /// <param name="secured">Whether the connection should be secured (https/wss).</param>
        public ConnectionEndpoint(string hostname, int port, bool secured = false)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.Secured = secured;
        }

        /// <summary>
        /// Gets the hash code of this endpoint.
        /// </summary>
        /// <returns>Hash code of this endpoint.</returns>
        public override int GetHashCode() => 13 + (7 * this.Hostname.GetHashCode()) + (7 * this.Port);

        /// <summary>
        /// Gets the string representation of this connection endpoint.
        /// </summary>
        /// <returns>String representation of this endpoint.</returns>
        public override string ToString() => $"{this.Hostname}:{this.Port}";

        internal string ToHttpString()
        {
            var secure = this.Secured ? "s" : "";
            return $"http{secure}://{this}";
        }

        internal string ToWebSocketString()
        {
            var secure = this.Secured ? "s" : "";
            return $"ws{secure}://{this}/";
        }
    }
}
