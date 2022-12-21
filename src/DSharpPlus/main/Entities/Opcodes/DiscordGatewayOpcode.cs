namespace DSharpPlus.Core.Enums
{
    public enum DiscordGatewayOpcode
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
