namespace DSharpPlus.Core.Enums
{
    public enum DiscordVoiceCloseEventCode
    {
        /// <summary>
        /// You sent an invalid <see cref="DiscordVoiceOpCode">opcode</see>.
        /// </summary>
        UnknownOpCode = 4001,

        /// <summary>
        /// You sent a invalid payload in your <see cref="Entities.Gateway.Commands.DiscordIdentifyCommand">identifying</see> to the Gateway.
        /// </summary>
        FailedToDecodePayload = 4002,

        /// <summary>
        /// You sent a payload before <see cref="Entities.Gateway.Commands.DiscordIdentifyCommand">identifying</see> with the Gateway.
        /// </summary>
        NotAuthenticated = 4003,

        /// <summary>
        /// The token you sent in your <see cref="Entities.Gateway.Commands.DiscordIdentifyCommand">identify</see> payload is incorrect.
        /// </summary>
        AuthenticationFailed = 4004,

        /// <summary>
        /// You sent more than one <see cref="Entities.Gateway.Commands.DiscordIdentifyCommand">identify</see> payload. Stahp.
        /// </summary>
        AlreadyAuthenticated = 4005,

        /// <summary>
        /// Your session is no longer valid.
        /// </summary>
        SessionNoLongerValid = 4006,

        /// <summary>
        /// Your session has timed out.
        /// </summary>
        SessionTimeout = 4009,

        /// <summary>
        /// We can't find the server you're trying to connect to.
        /// </summary>
        ServerNotFound = 4011,

        /// <summary>
        /// We didn't recognize the <see href="https://discord.com/developers/docs/topics/voice-connections#establishing-a-voice-udp-connection-example-select-protocol-payload">protocol</see> you sent.
        /// </summary>
        UnknownProtocol = 4012,

        /// <summary>
        /// Channel was deleted, you were kicked, voice server changed, or the main gateway session was dropped. Should not reconnect.
        /// </summary>
        Disconnected = 4014,

        /// <summary>
        /// The server crashed. Our bad! Try <see href="https://discord.com/developers/docs/topics/voice-connections#resuming-voice-connection">resuming</see>.
        /// </summary>
        VoiceServerCrashed = 4015,

        /// <summary>
        /// We didn't recognize your <see href="https://discord.com/developers/docs/topics/voice-connections#encrypting-and-sending-voice">encryption</see>.
        /// </summary>
        UnknownEncryptionMode = 4016
    }
}
