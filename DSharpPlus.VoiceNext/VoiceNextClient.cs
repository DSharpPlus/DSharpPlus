using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext.VoiceEntities;
using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext
{
    /// <summary>
    /// VoiceNext client.
    /// </summary>
    public sealed class VoiceNextClient : IModule
    {
        /// <summary>
        /// DiscordClient instance for this module.
        /// </summary>
        public DiscordClient Client { get { return this._client; } }
        private DiscordClient _client;

        private VoiceNextConfiguration Configuration { get; set; }

        private ConcurrentDictionary<ulong, VoiceNextConnection> ActiveConnections { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> VoiceStateUpdates { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> VoiceServerUpdates { get; set; }

        internal VoiceNextClient(VoiceNextConfiguration config)
        {
            this.Configuration = config;

            this.ActiveConnections = new ConcurrentDictionary<ulong, VoiceNextConnection>();
            this.VoiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
            this.VoiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
        }

        /// <summary>
        /// DO NOT RUN THIS MANUALLY.
        /// </summary>
        /// <param name="client"></param>
        public void Setup(DiscordClient client)
        {
            this._client = client;
            this.Client.VoiceStateUpdate += this.Client_VoiceStateUpdate;
            this.Client.VoiceServerUpdate += this.Client_VoiceServerUpdate;
        }

        /// <summary>
        /// Create a VoiceNext connection for the specified channel.
        /// </summary>
        /// <param name="channel">Channel to connect to.</param>
        /// <returns>VoiceNext connection for this channel.</returns>
        public async Task<VoiceNextConnection> ConnectAsync(DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Voice)
                throw new ArgumentException(nameof(channel), "Invalid channel specified; needs to be voice channel");

            if (channel.Parent == null)
                throw new ArgumentException(nameof(channel), "Invalid channel specified; needs to be guild channel");

            var gld = channel.Parent;
            if (ActiveConnections.ContainsKey(gld.Id))
                throw new InvalidOperationException("This guild already has a voice connection");

            var vstut = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
            var vsrut = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
            this.VoiceStateUpdates[gld.Id] = vstut;
            this.VoiceServerUpdates[gld.Id] = vsrut;

            var vsd = new VoiceDispatch
            {
                OpCode = 4,
                Payload = new VoiceStateUpdatePayload
                {
                    GuildId = gld.Id,
                    ChannelId = channel.Id,
                    Deafened = false,
                    Muted = false
                }
            };
            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            DiscordClient._websocket_client.SendMessage(vsj);
            
            var vstu = await vstut.Task;
            var vstup = new VoiceStateUpdatePayload
            {
                SessionId = vstu.SessionID,
                UserId = vstu.UserID
            };
            var vsru = await vsrut.Task;
            var vsrup = new VoiceServerUpdatePayload
            {
                Endpoint = vsru.Endpoint,
                GuildId = vsru.GuildID,
                Token = vsru.VoiceToken
            };

            var d1 = (TaskCompletionSource<VoiceStateUpdateEventArgs>)null;
            var d2 = (TaskCompletionSource<VoiceServerUpdateEventArgs>)null;
            this.VoiceStateUpdates.TryRemove(gld.Id, out d1);
            this.VoiceServerUpdates.TryRemove(gld.Id, out d2);
            
            var vnc = new VoiceNextConnection(this.Client, gld, channel, this.Configuration, vsrup, vstup);
            vnc.VoiceDisconnected += this.Vnc_VoiceDisconnected;
            await vnc.ConnectAsync();
            await vnc.WaitForReady();
            this.ActiveConnections[gld.Id] = vnc;
            return vnc;
        }

        /// <summary>
        /// Gets a VoiceNext connection for specified guild.
        /// </summary>
        /// <param name="guild">Guild to get VoiceNext connection for.</param>
        /// <returns>VoiceNext connection for the specified guild.</returns>
        public VoiceNextConnection GetConnection(DiscordGuild guild)
        {
            if (this.ActiveConnections.ContainsKey(guild.Id))
                return this.ActiveConnections[guild.Id];

            return null;
        }

        private void Vnc_VoiceDisconnected(DiscordGuild guild)
        {
            var vnc = (VoiceNextConnection)null;
            if (this.ActiveConnections.ContainsKey(guild.Id))
                this.ActiveConnections.TryRemove(guild.Id, out vnc);

            var vsd = new VoiceDispatch
            {
                OpCode = 4,
                Payload = new VoiceStateUpdatePayload
                {
                    GuildId = guild.Id,
                    ChannelId = null
                }
            };
            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            DiscordClient._websocket_client.SendMessage(vsj);
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
