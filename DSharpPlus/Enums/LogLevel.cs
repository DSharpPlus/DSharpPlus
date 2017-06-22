namespace DSharpPlus
{
    /// <summary>
    /// Represents information about log's verbosity level.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Signifies a debug-level message.
        /// </summary>
        Debug       = 8,

        /// <summary>
        /// Signifies info-level message.
        /// </summary>
        Info        = 4,

        /// <summary>
        /// Signifies warning-level message.
        /// </summary>
        Warning     = 2,

        /// <summary>
        /// Signifies error-level message.
        /// </summary>
        Error       = 1,

        /// <summary>
        /// Signifies critical error-level message.
        /// </summary>
        Critical    = 0
    }
}
