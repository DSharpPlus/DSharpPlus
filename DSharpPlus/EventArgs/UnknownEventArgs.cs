namespace DSharpPlus
{
    public class UnknownEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Event's name
        /// </summary>
        public string EventName { get; internal set; }
        /// <summary>
        /// Event's JSON
        /// </summary>
        public string Json { get; internal set; }

        public UnknownEventArgs(DiscordClient client) : base(client) { }
    }
}
