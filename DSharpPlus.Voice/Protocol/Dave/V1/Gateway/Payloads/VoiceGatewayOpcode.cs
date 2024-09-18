namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads;

/// <summary>
/// Enumerates opcodes to the v1 DAVE voice gateway.
/// </summary>
internal enum VoiceGatewayOpcode : byte
{
    /// <summary>
    /// Serverbound. Begins a websocket connection to the voice gateway.
    /// </summary>
    Identify,

    /// <summary>
    /// Serverbound. Selects the preferred voice protocol.
    /// </summary>
    SelectProtocol,

    /// <summary>
    /// Clientbound. Completes the websocket handshake.
    /// </summary>
    Ready,

    /// <summary>
    /// Serverbound. Sends a heartbeat to keep the socket connection alive.
    /// </summary>
    Heartbeat,

    /// <summary>
    /// Clientbound. Describes the present voice session.
    /// </summary>
    SessionDescription,

    /// <summary>
    /// Bidirectional. Indicates which users are speaking.
    /// </summary>
    Speaking,

    /// <summary>
    /// Clientbound. Sent to acknowledge a client <see cref="Heartbeat"/>.
    /// </summary>
    HeartbeatAck,

    /// <summary>
    /// Serverbound. Resumes a connection.
    /// </summary>
    Resume,

    /// <summary>
    /// Clientbound. Sent upon connecting to the gateway, instructing the client on how to heartbeat.
    /// </summary>
    Hello,

    /// <summary>
    /// Clientbound. Confirms a successful <see cref="Resume"/>.
    /// </summary>
    Resumed,

    /// <summary>
    /// Clientbound. Indicates that one or more clients have connected.
    /// </summary>
    ClientsConnected = 11,

    /// <summary>
    /// Clientbound. Indicates that one client has disconnected from the session.
    /// </summary>
    ClientDisconnected = 13,

    /// <summary>
    /// Clientbound. Indicates that the DAVE protocol is transitioning between protocol versions.
    /// </summary>
    PrepareTransition = 21,

    /// <summary>
    /// Clientbound. Executes the previously announced transition.
    /// </summary>
    ExecuteTransition,

    /// <summary>
    /// Serverbound. Indicates that the client is ready to commit to a previously announced transition.
    /// </summary>
    TransitionReady,

    /// <summary>
    /// Clientbound. Indicates that a protocol version or group change is inbound.
    /// </summary>
    PrepareEpoch,

    /// <summary>
    /// Clientbound. Contains the credentials and public key for the voice gateway external sender.
    /// </summary>
    MlsExternalSender,

    /// <summary>
    /// Serverbound. Contains the MLS key package for the pending group member.
    /// </summary>
    MlsKeyPackage,

    /// <summary>
    /// Clientbound. Contains MLS proposals to append or revoke.
    /// </summary>
    MlsProposals,

    /// <summary>
    /// Serverbound. Contains the MLS commit for this client with optional welcome message.
    /// </summary>
    MlsCommitWelcome,

    /// <summary>
    /// Clientbound. Announces that a commit shall be processed for an upcoming transition.
    /// </summary>
    MlsAnnounceCommitTransition,

    /// <summary>
    /// Clientbound. Announces a MLS welcome for an upcoming transition
    /// </summary>
    MlsWelcome,

    /// <summary>
    /// Serverbound. Flags an invalid MLS commit or welcome, requesting another attempt.
    /// </summary>
    MlsInvalidCommitWelcome
}
