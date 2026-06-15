namespace DSharpPlus.Net.Gateway;

/// <summary>
/// Represents standard websocket close statuses as well as Discord-specific close codes.
/// </summary>
public enum GatewayCloseCode
{
    /// <summary>
    /// The websocket was regularly closed.
    /// </summary>
    NormalClosure = 1000,

    /// <summary>
    /// The gateway server is shutting down.
    /// </summary>
    GoingAway = 1001,

    /// <summary>
    /// No reason specified.
    /// </summary>
    Empty = 1005,

    /// <summary>
    /// The connection timed out.
    /// </summary>
    Timeout = 3008,

    /// <summary>
    /// An unknown error occurred.
    /// </summary>
    UnknownError = 4000,

    /// <summary>
    /// An unknown payload was sent.
    /// </summary>
    UnknownOpcode = 4001,

    /// <summary>
    /// A payload could not be decoded.
    /// </summary>
    DecodeError = 4002,

    /// <summary>
    /// The session is not currently authenticated.
    /// </summary>
    NotAuthenticated = 4003,

    /// <summary>
    /// The provided token is incorrect.
    /// </summary>
    AuthenticationFailed = 4004,

    /// <summary>
    /// Attempted to authenticate to an already authenticated session.
    /// </summary>
    AlreadyAuthenticated = 4005,

    /// <summary>
    /// Invalid sequence number sent in attempting to resume.
    /// </summary>
    InvalidSequence = 4007,

    /// <summary>
    /// Attempted to send more than 120 events per second to the remote websocket.
    /// </summary>
    Ratelimited = 4008,

    /// <summary>
    /// The session timed out.
    /// </summary>
    SessionTimedOut = 4009,

    /// <summary>
    /// The provided shard info was invalid.
    /// </summary>
    InvalidShard = 4010,

    /// <summary>
    /// Attempted to connect with insufficient shards.
    /// </summary>
    ShardingRequired = 4011,

    /// <summary>
    /// Specified an invalid API version when connecting.
    /// </summary>
    InvalidAPIVersion = 4012,

    /// <summary>
    /// Sent an invalid intent.
    /// </summary>
    InvalidIntents = 4013,

    /// <summary>
    /// Requested an intent the bot is not authorized for.
    /// </summary>
    DisallowedIntents = 4014
}
