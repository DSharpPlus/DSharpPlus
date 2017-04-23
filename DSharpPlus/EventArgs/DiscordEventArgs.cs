using System;

namespace DSharpPlus
{
    public abstract class DiscordEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the client that triggered the event.
        /// </summary>
        public DiscordClient Client { get; internal set; }

        /// <summary>
        /// Gets the ID of the shard to which the client is connected.
        /// </summary>
        public int ShardId => this.Client.ShardId;

        protected DiscordEventArgs(DiscordClient client)
        {
            this.Client = client;
        }
    }
}
