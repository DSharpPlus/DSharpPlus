namespace DSharpPlus.EventArgs
{
    public abstract class DiscordEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the client that triggered the event.
        /// </summary>
        public DiscordClient Client { get; internal set; }

        protected DiscordEventArgs(DiscordClient client)
        {
            this.Client = client;
        }
    }
}
