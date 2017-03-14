using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.VoiceEntities;
using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext
{
    public sealed class VoiceNextClient : IModule
    {
        public DiscordClient Client { get { return this._client; } }
        private DiscordClient _client;

        private ConcurrentDictionary<ulong, VoiceNextConnection> ActiveConnections { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> VoiceStateUpdates { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> VoiceServerUpdates { get; set; }

        internal VoiceNextClient()
        {
            this.ActiveConnections = new ConcurrentDictionary<ulong, VoiceNextConnection>();
            this.VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
            this.VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
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

            var vstut = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
            var vsrut = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
            this.VoiceStateUpdates[gld.ID] = vstut;
            this.VoiceServerUpdates[gld.ID] = vsrut;

            var vsd = new VoiceDispatch
            {
                OpCode = 4,
                Payload = new VoiceStateUpdatePayload
                {
                    GuildId = gld.ID.ToString(),
                    ChannelId = channel.ID.ToString(),
                    Deafened = false,
                    Muted = false
                }
            };
            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            DiscordClient._websocketClient._socket.Send(vsj);
            
            var vstu = await vstut.Task;
            var vstup = new VoiceStateUpdatePayload
            {
                SessionId = vstu.SessionID,
                UserId = vstu.UserID.ToString()
            };
            var vsru = await vsrut.Task;
            var vsrup = new VoiceServerUpdatePayload
            {
                Endpoint = vsru.Endpoint,
                GuildId = vsru.GuildID.ToString(),
                Token = vsru.VoiceToken
            };

            this.VoiceStateUpdates.TryRemove(gld.ID, out _);
            this.VoiceServerUpdates.TryRemove(gld.ID, out _);

            var vnc = new VoiceNextConnection(this.Client, gld, channel, vsrup, vstup);
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
                this.ActiveConnections.TryRemove(guild.ID, out _);
        }

        private Task Client_VoiceStateUpdate(VoiceStateUpdateEventArgs e)
        {
            var gid = e.GuildID;
            if (gid == 0)
                return Task.Delay(0);

            if (this.VoiceStateUpdates.ContainsKey(gid))
                this.VoiceStateUpdates[gid].SetResult(e);

            return Task.Delay(0);
        }

        private Task Client_VoiceServerUpdate(VoiceServerUpdateEventArgs e)
        {
            var gid = e.GuildID;
            if (this.VoiceServerUpdates.ContainsKey(gid))
                this.VoiceServerUpdates[gid].SetResult(e);

            return Task.Delay(0);
        }
    }
}
