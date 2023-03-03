// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
