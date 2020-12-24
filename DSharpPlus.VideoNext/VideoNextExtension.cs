using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VideoNext.Entities;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.VideoNext
{
    public class VideoNextExtension: BaseExtension
    {
        private VideoNextConfiguration Configuration { get; set; }
        
        //As of right now, only a single active connection can be established, due to limitations of the op-code 22 payload.
        private ConcurrentDictionary<ulong, VideoNextConnection> ActiveConnections { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<StreamCreateEventArgs>> StreamCreatedUpdates { get; set; }
        private ConcurrentDictionary<ulong, TaskCompletionSource<StreamUpdateEventArgs>> StreamUpdates { get; set; }

        /// <summary>
        /// Determines whether or not this extension has receive capabilities enabled.
        /// </summary>
        public bool ReceiveEnabled
            => Configuration.EnableIncoming;
        
        internal VideoNextExtension(VideoNextConfiguration config)
        {
            this.Configuration = new VideoNextConfiguration(config);
            
            this.ActiveConnections = new ConcurrentDictionary<ulong, VideoNextConnection>();
            this.StreamCreatedUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<StreamCreateEventArgs>>();
            this.StreamUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<StreamUpdateEventArgs>>();
        }

        /// <summary>
        /// DO not use manually.
        /// </summary>
        /// <param name="client">Used internally only.</param>
        /// <exception cref="InvalidOperationException"/>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException();
            this.Client = client;
            
            this.Client.StreamCreated += this.Client_StreamCreated;
            this.Client.StreamServerUpdated += this.Client_StreamUpdated;
        }
        
        /// <summary>
        /// Creates a VideoNext connection for the specified channel.
        /// </summary>
        /// <param name="channel">The channel to connect to</param>
        /// <exception cref="ArgumentException">Thrown when a non-video channel is specified.</exception>
        /// <returns>The VideoNext connection for the specified channel.</returns>
        public async Task<VideoNextConnection> ConnectAsync(DiscordChannel channel)
        {
            if(channel.Type != ChannelType.Voice)
                throw new ArgumentException("Invalid channel specified; Must be a voice channel!", nameof(channel));
            if(channel.Guild == null)
                throw new ArgumentException("Invalid channel specified! This channel is not a guild channel!");
            
            /*
            if(!channel.PermissionsFor(channel.Guild.CurrentMember).HasPermission(Permissions.Stream))
                throw new InvalidOperationException("This bot requires STREAM permission to stream on this channel.");
            */

            //Verifies that the voice is already connected. If not, connects to voice.
            if (this.Client.GetVoiceNext()?.GetConnection(channel.Guild) == null)
            {
                await this.Client.GetVoiceNext().ConnectAsync(channel);
            }
            
            if(ActiveConnections.ContainsKey(channel.Guild.Id))
                throw new InvalidOperationException("Already connected to this guild!");
            
            var sce = new TaskCompletionSource<StreamCreateEventArgs>();
            var sue = new TaskCompletionSource<StreamUpdateEventArgs>();
            
            this.StreamCreatedUpdates[channel.Guild.Id] = sce;
            this.StreamUpdates[channel.Guild.Id] = sue;
            
            //Send a VoiceNext payload to indicate that self video should be enabled.
            var vsd = new 
            {
                op = 4,
                d = new
                {
                    guild_id = channel.Guild.Id,
                    channel_id = channel.Id,
                    user_id = Client.CurrentUser.Id,
                    self_deaf = false,
                    self_mute = false,
                    //self_video = true
                }
            };

            await Client.GetVoiceNext().GetConnection(channel.Guild).WsSendAsync(JsonConvert.SerializeObject(vsd, Formatting.None)).ConfigureAwait(false);
            
            //Indicate that we wish to start a stream.
            var vc = new
            {
                op = 18,
                d = new
                {
                    type = "guild",
                    guild_id = channel.Guild.Id,
                    channel_id = channel.Id,
                    preferred_region = "us-east",
                }
            };
            await Client.WsSendAsync(JsonConvert.SerializeObject(vc, Formatting.None)).ConfigureAwait(false);
            
            
            var sc = await sce.Task;
            var su = await sue.Task;

            VideoStateData data = new VideoStateData
            {    
                Token = su.Token,
                Key = sc.Key,
                Endpoint = su.Endpoint,
                RtcServerId = sc.RtcServerId,
                Deafened = su.Deafened
            };
            
            var vnc = new VideoNextConnection(Client, channel, Configuration, data);
            //vnc.VideoDisconnected += this.Vnc_VideoDisconnected;
            await vnc.ConnectAsync().ConfigureAwait(false);
            await vnc.WaitForReadyAsync().ConfigureAwait(false);
            this.ActiveConnections[channel.Guild.Id] = vnc;
            return vnc;
        }
        
        /// <summary>
        /// Gets a VoiceNext connection for specified guild.
        /// </summary>
        /// <param name="guild">Guild to get VoiceNext connection for.</param>
        /// <returns>VoiceNext connection for the specified guild.</returns>
        public VideoNextConnection GetConnection(DiscordGuild guild)
        {
            if (this.ActiveConnections.ContainsKey(guild.Id))
                return this.ActiveConnections[guild.Id];

            return null;
        }

        internal async Task Client_StreamCreated(DiscordClient client, StreamCreateEventArgs e)
        {
            var item = StreamCreatedUpdates.TryGetValue(ulong.Parse(e.Key.Split(':')[1]), out var tsk);
            if (item)
            {
                StreamCreatedUpdates[ulong.Parse(e.Key.Split(':')[1])].SetResult(e);
            }
        }

        internal async Task Client_StreamUpdated(DiscordClient client, StreamUpdateEventArgs su)
        {
            var item = StreamUpdates.TryGetValue(ulong.Parse(su.Key.Split(':')[1]), out var tsk);
            if (item)
            {
                StreamUpdates[ulong.Parse(su.Key.Split(':')[1])].SetResult(su);
            }
        }
    }
}