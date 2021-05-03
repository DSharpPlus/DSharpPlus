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

using System.Threading.Tasks;

namespace DSharpPlus.Net.Udp
{
    /// <summary>
    /// Creates an instance of a UDP client implementation.
    /// </summary>
    /// <returns>Constructed UDP client implementation.</returns>
    public delegate BaseUdpClient UdpClientFactoryDelegate();

    /// <summary>
    /// Represents a base abstraction for all UDP client implementations.
    /// </summary>
    public abstract class BaseUdpClient
    {
        /// <summary>
        /// Configures the UDP client.
        /// </summary>
        /// <param name="endpoint">Endpoint that the client will be communicating with.</param>
        public abstract void Setup(ConnectionEndpoint endpoint);

        /// <summary>
        /// Sends a datagram.
        /// </summary>
        /// <param name="data">Datagram.</param>
        /// <param name="dataLength">Length of the datagram.</param>
        /// <returns></returns>
        public abstract Task SendAsync(byte[] data, int dataLength);

        /// <summary>
        /// Receives a datagram.
        /// </summary>
        /// <returns>The received bytes.</returns>
        public abstract Task<byte[]> ReceiveAsync();

        /// <summary>
        /// Closes and disposes the client.
        /// </summary>
        public abstract void Close();
    }
}
