namespace DSharpPlus.Voice.Protocol.Gateway;

/// <summary>
/// Represents a close code received from the voice gateway.
/// </summary>
public enum VoiceGatewayCloseCode
{
    /// <summary>
    /// The connection was regularly closed - resume.
    /// </summary>
    NormalClosure = 1000,

    /// <summary>
    /// The endpoint is currently unavailable - do not reconnect.
    /// </summary>
    EndpointUnavailable = 1001,

    /// <summary>
    /// We sent something with an invalid message type. This indicates a library bug - do not reconnect.
    /// </summary>
    InvalidMessageType = 1003,

    /// <summary>
    /// No error specified - try resuming?
    /// </summary>
    Empty = 1005,

    /// <summary>
    /// We sent something with an invalid message type (but rephrased). This indicates a library bug - do not reconnect.
    /// </summary>
    InvalidPayloadData = 1007,

    /// <summary>
    /// We seem to have done something bad - try resuming?
    /// </summary>
    PolicyViolation = 1008,

    /// <summary>
    /// This should be impossible to hit and probably indicates a library bug - do not reconnect.
    /// </summary>
    MessageTooBig = 1009,

    /// <summary>
    /// An unspecified error occurred on the voice server - try resuming?
    /// </summary>
    InternalServerError = 1011,

    /// <summary>
    /// We sent an unknown opcode. This indicates a library bug - do not reconnect.
    /// </summary>
    UnknownOpcode = 4001,

    /// <summary>
    /// We sent an invalid IDENTIFY payload. This indicates a library bug - do not reconnect.
    /// </summary>
    FailedToAuthenticate = 4002,

    /// <summary>
    /// We sent a payload before identifying. This indicates a library bug - do not reconnect.
    /// </summary>
    NotAuthenticated = 4003,

    /// <summary>
    /// The token we sent in IDENTIFY was incorrect or invalid. This indicates that the voice session we tried to connect to has since
    /// ceased to exist - do not reconnect.
    /// </summary>
    AuthenticationFailed = 4004,

    /// <summary>
    /// We sent IDENTIFY twice. This indicates a library bug - do not reconnect.
    /// </summary>
    AlreadyAuthenticated = 4005,

    /// <summary>
    /// The voice session is invalid - do not reconnect.
    /// </summary>
    SessionInvalid = 4006,

    /// <summary>
    /// The voice session has timed out - do not reconnect.
    /// </summary>
    SessionTimeout = 4009,

    /// <summary>
    /// The server we're trying to connect to doesn't exist. This indicates that the voice session we tried to connect to has since
    /// ceased to exist - do not reconnect.
    /// </summary>
    ServerNotFound = 4011,

    /// <summary>
    /// We sent an invalid audio protocol. This indicates a library bug - do not reconnect.
    /// </summary>
    UnknownProtocol = 4012,

    /// <summary>
    /// We were kicked from the channel - do not reconnect.
    /// </summary>
    Disconnected = 4014,

    /// <summary>
    /// The voice server crashed - resume.
    /// </summary>
    VoiceServerCrashed = 4015,

    /// <summary>
    /// We specified an unknown encryption mode - try reconnecting, we can renegotiate that.
    /// </summary>
    UnknownEncryptionMode = 4016,

    /// <summary>
    /// We failed to apply end-to-end encryption. This indicates a library bug - do not reconnect.
    /// </summary>
    E2EERequired = 4017,

    /// <summary>
    /// We sent a malformed payload to the voice gateway - reconnect.
    /// </summary>
    BadRequest = 4020,

    /// <summary>
    /// We were ratelimited - do not reconnect.
    /// </summary>
    // unironically i did not know you could get ratelimited on connecting to voice channels, or whatever this ratelimit is actually for?
    // can't really be for the voice gateway considering that ought to be reconnectable, but the docs tell us not to reconnect here
    RateLimited = 4021,

    /// <summary>
    /// The call was forcibly terminated - do not reconnect.
    /// </summary>
    CallTerminated = 4022
}