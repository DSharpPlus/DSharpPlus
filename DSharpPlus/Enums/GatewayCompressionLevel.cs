namespace DSharpPlus
{
    /// <summary>
    /// Determines at which level should the WebSocket traffic be compressed.
    /// </summary>
    public enum GatewayCompressionLevel : byte
    {
        /// <summary>
        /// Defines that traffic should not be compressed at all.
        /// </summary>
        None = 0,

        /// <summary>
        /// Defines that traffic should be compressed at payload level.
        /// </summary>
        Payload = 1,

        /// <summary>
        /// Defines that entire traffic stream should be compressed.
        /// </summary>
        Stream = 2
    }
}
