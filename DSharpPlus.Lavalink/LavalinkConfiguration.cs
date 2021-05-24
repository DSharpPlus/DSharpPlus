// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DSharpPlus.Entities;
using DSharpPlus.Net;

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
        /// Sets whether the connection wrapper should attempt automatic reconnects should the connection drop.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool SocketAutoReconnect { internal get; set; } = true;

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
        /// Sets the time in miliseconds to wait for Lavalink's voice WebSocket to close after leaving a voice channel.
        /// <para>This will be the delay before the guild connection is removed.</para>
        /// <para>Defaults to 3000 miliseconds.</para>
        /// </summary>
        public int WebSocketCloseTimeout { internal get; set; } = 3000;

        /// <summary>
        /// Sets the voice region ID for the Lavalink connection.
        /// <para>This should be used if nodes should be filtered by region with <see cref="LavalinkExtension.GetIdealNodeConnection(DiscordVoiceRegion)"/>.</para>
        /// </summary>
        public DiscordVoiceRegion Region { internal get; set; }

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
                Port = other.RestEndpoint.Port,
                Secured = other.RestEndpoint.Secured
            };
            this.SocketEndpoint = new ConnectionEndpoint
            {
                Hostname = other.SocketEndpoint.Hostname,
                Port = other.SocketEndpoint.Port,
                Secured = other.SocketEndpoint.Secured
            };
            this.Password = other.Password;
            this.ResumeKey = other.ResumeKey;
            this.ResumeTimeout = other.ResumeTimeout;
            this.SocketAutoReconnect = other.SocketAutoReconnect;
            this.Region = other.Region;
            this.WebSocketCloseTimeout = other.WebSocketCloseTimeout;
        }
    }
}
