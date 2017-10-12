namespace DSharpPlus.EventArgs
{
    public abstract class DiscordEventArgs : System.EventArgs
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
