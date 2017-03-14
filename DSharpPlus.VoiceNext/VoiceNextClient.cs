using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.VoiceNext
{
    public sealed class VoiceNextClient : IModule
    {
        public DiscordClient Client { get; }
        private DiscordClient _client;

        private Dictionary<ulong, VoiceNextConnection> ActiveConnections { get; set; }

        internal VoiceNextClient()
        {
            this.ActiveConnections = new Dictionary<ulong, VoiceNextConnection>();
        }

        public void Setup(DiscordClient client)
        {
            this._client = client;
            this.Client.VoiceStateUpdate += this.Client_VoiceStateUpdate;
            this.Client.VoiceServerUpdate += this.Client_VoiceServerUpdate;
        }

        public async Task<VoiceNextConnection> ConnectAsync(DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Voice)
                throw new ArgumentException(nameof(channel), "Invalid channel specified; needs to be voice channel");

            if (channel.Parent == null)
                throw new ArgumentException(nameof(channel), "Invalid channel specified; needs to be guild channel");

            var gld = channel.Parent;
            if (ActiveConnections.ContainsKey(gld.ID))
                throw new InvalidOperationException("This guild already has a voice connection");

            // send voice state update
            // wait for both events

            var vnc = new VoiceNextConnection(this.Client, gld, channel, null, null);
            vnc.VoiceDisconnected += this.Vnc_VoiceDisconnected;
            return vnc;
        }

        public VoiceNextConnection GetConnection(DiscordGuild guild)
        {
            if (this.ActiveConnections.ContainsKey(guild.ID))
                return this.ActiveConnections[guild.ID];

            return null;
        }

        private void Vnc_VoiceDisconnected(DiscordGuild guild)
        {
            if (this.ActiveConnections.ContainsKey(guild.ID))
                this.ActiveConnections.Remove(guild.ID);
        }

        private Task Client_VoiceStateUpdate(VoiceStateUpdateEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task Client_VoiceServerUpdate(VoiceServerUpdateEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
