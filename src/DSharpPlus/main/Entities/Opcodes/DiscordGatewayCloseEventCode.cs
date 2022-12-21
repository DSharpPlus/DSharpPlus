namespace DSharpPlus.Entities;

public enum DiscordGatewayCloseEventCode
{
    /// <summary>
    /// We're not sure what went wrong. Try reconnecting?
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    UnknownError = 4000,

    /// <summary>
    /// You sent an invalid <see cref="DiscordGatewayOpCode"/> or an invalid payload for an opcode. Don't do that!
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    UnknownOpCode = 4001,

    /// <summary>
    /// You sent an invalid <see cref="Entities.DiscordGatewayPayload"/> to us. Don't do that!
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    DecodeError = 4002,

    /// <summary>
    /// You sent us a payload prior to <see cref="DiscordGatewayOpCode.Identify"/>. Don't do that!
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    NotAuthenticated = 4003,

    /// <summary>
    /// The account token sent with your <see cref="DiscordGatewayOpCode.Identify"/> payload is incorrect.
    /// </summary>
    /// <remarks>
    /// Do not reconnect.
    /// </remarks>
    AuthenticationFailed = 4004,

    /// <summary>
    /// You sent more than one identify payload. Don't do that!
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    AlreadyAuthenticated = 4005,

    /// <summary>
    /// The sequence sent when <see cref="Entities.Gateway.Commands.DiscordResumeCommand"/> (resuming) the session was invalid. Reconnect and start a new session.
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    InvalidSeq = 4007,

    /// <summary>
    /// Woah nelly! You're sending payloads to us too quickly. Slow it down! You will be disconnected on receiving this.
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    RateLimited = 4008,

    /// <summary>
    /// Your session timed out. Reconnect and start a new one.
    /// </summary>
    /// <remarks>
    /// Reconnect.
    /// </remarks>
    SessionTimedOut = 4009,

    /// <summary>
    /// You sent us an invalid shard when identifying.
    /// </summary>
    /// <remarks>
    /// Do not reconnect.
    /// </remarks>
    InvalidShard = 4010,

    /// <summary>
    /// The session would have handled too many guilds - you are required to shard your connection in order to connect.
    /// </summary>
    /// <remarks>
    /// Do not reconnect.
    /// </remarks>
    ShardingRequired = 4011,

    /// <summary>
    /// You sent an invalid version for the gateway.
    /// </summary>
    /// <remarks>
    /// Do not reconnect.
    /// </remarks>
    InvalidAPIVersion = 4012,

    /// <summary>
    /// You sent an invalid intent for a <see cref="DiscordGatewayIntents"/>. You may have incorrectly calculated the bitwise value.
    /// </summary>
    /// <remarks>
    /// Do not reconnect.
    /// </remarks>
    InvalidIntents = 4013,

    /// <summary>
    /// You sent a disallowed intent for a <see cref="DiscordGatewayIntents"/>. You may have tried to specify an intent that you <see href="https://discord.com/developers/docs/topics/gateway#privileged-intents">have not enabled or are not approved for</see>.
    /// </summary>
    /// <remarks>
    /// Do not reconnect.
    /// </remarks>
    DisallowedIntents = 4014
}
