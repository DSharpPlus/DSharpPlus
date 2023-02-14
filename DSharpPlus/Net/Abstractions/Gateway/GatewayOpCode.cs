// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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

namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Specifies an OP code in a gateway payload.
    /// </summary>
    internal enum GatewayOpCode : int
    {
        /// <summary>
        /// Used for dispatching events.
        /// </summary>
        Dispatch = 0,

        /// <summary>
        /// Used for pinging the gateway or client, to ensure the connection is still alive.
        /// </summary>
        Heartbeat = 1,

        /// <summary>
        /// Used for initial handshake with the gateway.
        /// </summary>
        Identify = 2,

        /// <summary>
        /// Used to update client status.
        /// </summary>
        StatusUpdate = 3,

        /// <summary>
        /// Used to update voice state, when joining, leaving, or moving between voice channels.
        /// </summary>
        VoiceStateUpdate = 4,

        /// <summary>
        /// Used for pinging the voice gateway or client, to ensure the connection is still alive.
        /// </summary>
        VoiceServerPing = 5,

        /// <summary>
        /// Used to resume a closed connection.
        /// </summary>
        Resume = 6,

        /// <summary>
        /// Used to notify the client that it has to reconnect.
        /// </summary>
        Reconnect = 7,

        /// <summary>
        /// Used to request guild members.
        /// </summary>
        RequestGuildMembers = 8,

        /// <summary>
        /// Used to notify the client about an invalidated session.
        /// </summary>
        InvalidSession = 9,

        /// <summary>
        /// Used by the gateway upon connecting.
        /// </summary>
        Hello = 10,

        /// <summary>
        /// Used to acknowledge a heartbeat.
        /// </summary>
        HeartbeatAck = 11,

        /// <summary>
        /// Used to request guild synchronization.
        /// </summary>
        GuildSync = 12
    }
}
