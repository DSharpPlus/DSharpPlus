namespace DSharpPlus.EventArgs
{
    public abstract class DiscordEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the client that triggered the event.
        /// </summary>
        public BaseDiscordClient Client { get; internal set; }

        protected DiscordEventArgs(BaseDiscordClient client)
        {
            this.Client = client;
        }
    }
}
