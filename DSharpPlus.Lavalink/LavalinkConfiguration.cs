using DSharpPlus.Net.Udp;

namespace DSharpPlus.Lavalink
{
    /// <summary>
    /// Lavalink connection configuration.
    /// </summary>
    public sealed class LavalinkConfiguration
    {
        /// <summary>
        /// Sets the endpoint for Lavalink REST.
        /// </summary>
        public ConnectionEndpoint RestEndpoint { internal get; set; } = new ConnectionEndpoint { Hostname = "127.0.0.1", Port = 2333 };

        /// <summary>
        /// Sets the endpoint for Lavalink Websocket connection.
        /// </summary>
        public ConnectionEndpoint SocketEndpoint { internal get; set; } = new ConnectionEndpoint { Hostname = "127.0.0.1", Port = 80 };

        /// <summary>
        /// Sets the password for Lavalink connection.
        /// </summary>
        public string Password { internal get; set; } = "youshallnotpass";

        /// <summary>
        /// Creates a new instance of <see cref="LavalinkConfiguration"/>.
        /// </summary>
        public LavalinkConfiguration() { }

        /// <summary>
        /// Creates a new instance of <see cref="LavalinkConfiguration"/>, copying the properties of another configuration.
        /// </summary>
        /// <param name="other">Configuration the properties of which are to be copied.</param>
        public LavalinkConfiguration(LavalinkConfiguration other)
        {
            this.RestEndpoint = new ConnectionEndpoint
            {
                Hostname = other.RestEndpoint.Hostname,
                Port = other.RestEndpoint.Port
            };
            this.SocketEndpoint = new ConnectionEndpoint
            {
                Hostname = other.SocketEndpoint.Hostname,
                Port = other.SocketEndpoint.Port
            };
            this.Password = other.Password;
        }
    }
}
