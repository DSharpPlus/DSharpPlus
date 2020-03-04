﻿using DSharpPlus.Net;

namespace DSharpPlus.Lavalink
{
    /// <summary>
    /// Lavalink connection configuration.
    /// </summary>
    public sealed class LavalinkConfiguration
    {
        /// <summary>
        /// Sets the endpoint for Lavalink REST.
        /// <para>Defaults to 127.0.0.1 on port 2333.</para>
        /// </summary>
        public ConnectionEndpoint RestEndpoint { internal get; set; } = new ConnectionEndpoint("127.0.0.1", 2333);

        /// <summary>
        /// Sets the endpoint for the Lavalink Websocket connection.
        /// <para>Defaults to 127.0.0.1 on port 2333.</para>
        /// </summary>
        public ConnectionEndpoint SocketEndpoint { internal get; set; } = new ConnectionEndpoint("127.0.0.1", 2333);

        /// <summary>
        /// Sets the password for the Lavalink connection.
        /// <para>Defaults to "youshallnotpass".</para>
        /// </summary>
        public string Password { internal get; set; } = "youshallnotpass";

        /// <summary>
        /// Sets the resume key for the Lavalink connection.
        /// <para>This will allow existing voice sessions to continue for a certain time after the client is disconnected.</para>
        /// </summary>
        public string ResumeKey { internal get; set; }

        /// <summary>
        /// Sets the time in seconds when all voice sessions are closed after the client disconnects.
        /// <para>Defaults to 60 seconds.</para>
        /// </summary>
        public int ResumeTimeout { internal get; set; } = 60;

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
            this.ResumeKey = other.ResumeKey;
            this.ResumeTimeout = other.ResumeTimeout;
        }
    }
}
