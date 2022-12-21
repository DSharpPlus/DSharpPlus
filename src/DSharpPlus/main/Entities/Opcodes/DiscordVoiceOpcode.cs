namespace DSharpPlus.Entities;

public enum DiscordVoiceOpcode
{
    /// <summary>
    /// Begin a voice websocket connection.
    /// </summary>
    /// <remarks>
    /// Sent by the client.
    /// </remarks>
    Identify = 0,

    /// <summary>
    /// Select the voice protocol.
    /// </summary>
    /// <remarks>
    /// Sent by the client.
    /// </remarks>
    SelectProtocol = 1,

    /// <summary>
    /// Complete the websocket handshake.
    /// </summary>
    /// <remarks>
    /// Sent by the server.
    /// </remarks>
    Ready = 2,

    /// <summary>
    /// Keep the websocket connection alive.
    /// </summary>
    /// <remarks>
    /// Sent by the client.
    /// </remarks>
    Heartbeat = 3,

    /// <summary>
    /// Describe the session.
    /// </summary>
    /// <remarks>
    /// Sent by the server.
    /// </remarks>
    SessionDescription = 4,

    /// <summary>
    /// Indicate which users are speaking.
    /// </summary>
    /// <remarks>
    /// Sent by the client and server.
    /// </remarks>
    Speaking = 5,

    /// <summary>
    /// Sent to acknowledge a received client heartbeat.
    /// </summary>
    /// <remarks>
    /// Sent by the server.
    /// </remarks>
    HeartbeatAck = 6,

    /// <summary>
    /// Resume a connection.
    /// </summary>
    /// <remarks>
    /// Sent by the client.
    /// </remarks>
    Resume = 7,

    /// <summary>
    /// Time to wait between sending heartbeats in milliseconds.
    /// </summary>
    /// <remarks>
    /// Sent by the server.
    /// </remarks>
    Hello = 8,

    /// <summary>
    /// Acknowledge a successful session resume.
    /// </summary>
    /// <remarks>
    /// Sent by the server.
    /// </remarks>
    Resumed = 9,

    /// <summary>
    /// A client has disconnected from the voice channel
    /// </summary>
    /// <remarks>
    /// Sent by the server.
    /// </remarks>
    ClientDisconnect = 13
}
