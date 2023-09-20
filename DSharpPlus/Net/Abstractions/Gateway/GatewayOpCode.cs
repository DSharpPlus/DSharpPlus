namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Specifies an OP code in a gateway payload.
    /// </summary>
    public enum GatewayOpCode : int
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
