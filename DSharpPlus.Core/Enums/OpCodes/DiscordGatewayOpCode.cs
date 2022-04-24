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

namespace DSharpPlus.Core.Enums
{
    public enum DiscordGatewayOpCode
    {
        /// <summary>
        /// An event was dispatched.
        /// </summary>
        /// <remarks>
        /// Recieved from the gateway.
        /// </remarks>
        Dispatch = 0,

        /// <summary>
        /// Fired periodically by the client to keep the connection alive.
        /// </summary>
        /// <remarks>
        /// Sent to and recieved from the gateway.
        /// </remarks>
        Heartbeat = 1,

        /// <summary>
        /// Starts a new session during the initial handshake.
        /// </summary>
        /// <remarks>
        /// Sent to the gateway.
        /// </remarks>
        Identify = 2,

        /// <summary>
        /// Update the client's presence.
        /// </summary>
        /// <remarks>
        /// Sent to the gateway.
        /// </remarks>
        PresenceUpdate = 3,

        /// <summary>
        /// Used to join/leave or move between voice channels.
        /// </summary>
        /// <remarks>
        /// Sent to the gateway.
        /// </remarks>
        VoiceStateUpdate = 4,

        /// <summary>
        /// Resume a previous session that was disconnected.
        /// </summary>
        /// <remarks>
        /// Sent to the gateway.
        /// </remarks>
        Resume = 6,

        /// <summary>
        /// You should attempt to reconnect and resume immediately.
        /// </summary>
        /// <remarks>
        /// Sent to and recieved from the gateway.
        /// </remarks>
        Reconnect = 7,

        /// <summary>
        /// Request information about offline guild members in a large guild.
        /// </summary>
        /// <remarks>
        /// Sent to the gateway.
        /// </remarks>
        RequestGuildMembers = 8,

        /// <summary>
        /// The session has been invalidated. You should reconnect and identify/resume accordingly.
        /// </summary>
        /// <remarks>
        /// Recieved from the gateway.
        /// </remarks>
        InvalidSession = 9,

        /// <summary>
        /// Sent immediately after connecting, contains the <c>heartbeat_interval</c> to use.
        /// </summary>
        /// <remarks>
        /// Recieved from the gateway.
        /// </remarks>
        Hello = 10,

        /// <summary>
        /// Sent in response to receiving a heartbeat to acknowledge that it has been received.
        /// </summary>
        /// <remarks>
        /// Recieved from the gateway.
        /// </remarks>
        HeartbeatACK = 11
    }
}
