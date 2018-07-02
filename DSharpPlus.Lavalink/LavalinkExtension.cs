using DSharpPlus.EventArgs;
using DSharpPlus.Net.Udp;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DSharpPlus.Lavalink
{
    public sealed class LavalinkExtension : BaseExtension
    {
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> VoiceStateUpdates { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> VoiceServerUpdates { get; set; }

        /// <summary>
        /// Creates a new instance of this Lavalink extension.
        /// </summary>
        internal LavalinkExtension()
        { }

        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="InvalidOperationException"/>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            this.Client.VoiceStateUpdated += this.Client_VoiceStateUpdate;
            this.Client.VoiceServerUpdated += this.Client_VoiceServerUpdate;
            this._connected = false;
        }

        /// <summary>
        /// Connect to a Lavalink node.
        /// </summary>
        /// <param name="server">Address and port of the Lavalink server to connect to.</param>
        /// <param name="password">Password for the server.</param>
        /// <returns>The established Lavalink connection.</returns>
        public async Task<LavalinkConnection> ConnectAsync(ConnectionEndpoint server, string password)
        {
            throw new NotImplementedException();
        }

        private Task Client_VoiceStateUpdate(VoiceStateUpdateEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task Client_VoiceServerUpdate(VoiceServerUpdateEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
