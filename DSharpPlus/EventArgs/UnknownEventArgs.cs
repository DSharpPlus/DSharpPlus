namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.UnknownEvent"/> event.
    /// </summary>
    public class UnknownEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the event's name.
        /// </summary>
        public string EventName { get; internal set; }

        /// <summary>
        /// Gets the event's data.
        /// </summary>
        public string Json { get; internal set; }

        internal UnknownEventArgs() : base() { }
    }
}
