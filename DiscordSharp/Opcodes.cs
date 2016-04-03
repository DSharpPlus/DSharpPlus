namespace DiscordSharp
{
    public static class Opcodes
    {
        /// <summary>
        /// Received only. When a regular event is dispatched (defined in T)
        /// </summary>
        public const int DISPATCH = 0;

        /// <summary>
        /// Received and sent out to verify a connection is still alive.
        /// </summary>
        public const int HEARTBEAT = 1;

        /// <summary>
        /// Sent only: initiates and starts a new connection
        /// </summary>
        public const int IDENTIFY = 2;

        /// <summary>
        /// Sent only: Updates the presence of the current user.
        /// </summary>
        public const int PRESENCE = 3;

        /// <summary>
        /// Sent only: initiates a connection to a voice server or updates an existing one.
        /// </summary>
        public const int VOICE_STATE = 4;

        /// <summary>
        /// Sent only: for the ping time to the voice server.
        /// </summary>
        public const int VOICE_PING = 5;

        /// <summary>
        /// Sent only: Second way to initiate a connection. Resumes an existing one.
        /// </summary>
        public const int RESUME = 6;

        /// <summary>
        /// Received only: tells the client to reconnect to a new gateway.
        /// </summary>
        public const int RECONNECT = 7;

        /// <summary>
        /// Sent only: asks the server for a list of particular members in a guild.
        /// </summary>
        public const int REQUEST_MEMBERS = 8;

        /// <summary>
        /// Received only: tells the client to invalidate and re-identify.
        /// </summary>
        public const int INVALIDATE_SESSION = 9;
    }
}
