using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink.VoiceEntities;
using DSharpPlus.Net.Udp;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Lavalink
{
    public sealed class LavalinkExtension : BaseExtension
    {
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> VoiceStateUpdates { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> VoiceServerUpdates { get; set; }

        private bool _connected;

        /// <summary>
        /// Creates a new instance of this Lavalink extension.
        /// </summary>
        /// <param name="config">Configuration for this extension.</param>
        internal LavalinkExtension(LavalinkConfiguration config)
        {

        }

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
        /// Connect to Lavalink.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disconnect from Lavalink.
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
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
