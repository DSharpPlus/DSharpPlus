using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace DSharpPlus.VideoNext
{
   public static class DiscordClientExtensions
    {
        /// <summary>
        /// Creates a new VoiceNext client with default settings.
        /// </summary>
        /// <param name="client">Discord client to create VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VideoNextExtension UseVideoNext(this DiscordClient client) 
            => UseVideoNext(client, new VideoNextConfiguration());

        /// <summary>
        /// Creates a new VoiceNext client with specified settings.
        /// </summary>
        /// <param name="client">Discord client to create VoiceNext instance for.</param>
        /// <param name="config">Configuration for the VoiceNext client.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VideoNextExtension UseVideoNext(this DiscordClient client, VideoNextConfiguration config)
        {
            if (client.GetExtension<VideoNextExtension>() != null)
                throw new InvalidOperationException("VoiceNext is already enabled for that client.");

            var vnext = new VideoNextExtension(config);
            client.AddExtension(vnext);
            return vnext;
        }

        /// <summary>
        /// Creates new VoiceNext clients on all shards in a given sharded client.
        /// </summary>
        /// <param name="client">Discord sharded client to create VoiceNext instances for.</param>
        /// <param name="config">Configuration for the VoiceNext clients.</param>
        /// <returns>A dictionary of created VoiceNext clients.</returns>
        public static async Task<IReadOnlyDictionary<int, VideoNextExtension>> UseVoiceNextAsync(this DiscordShardedClient client, VideoNextConfiguration config)
        {
            var modules = new Dictionary<int, VideoNextExtension>();
            await client.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                var vnext = shard.GetExtension<VideoNextExtension>();
                if (vnext == null)
                    vnext = shard.UseVideoNext(config);

                modules[shard.ShardId] = vnext;
            }

            return new ReadOnlyDictionary<int, VideoNextExtension>(modules);
        }

        /// <summary>
        /// Gets the active instance of VoiceNext client for the DiscordClient.
        /// </summary>
        /// <param name="client">Discord client to get VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VideoNextExtension GetVideoNext(this DiscordClient client) 
            => client.GetExtension<VideoNextExtension>();

        /// <summary>
        /// Retrieves a <see cref="VideoNextExtension"/> instance for each shard.
        /// </summary>
        /// <param name="client">The shard client to retrieve <see cref="VideoNextExtension"/> instances from.</param>
        /// <returns>A dictionary containing <see cref="VideoNextExtension"/> instances for each shard.</returns>
        public static async Task<IReadOnlyDictionary<int, VideoNextExtension>> GetVoiceNextAsync(this DiscordShardedClient client)
        {
            await client.InitializeShardsAsync().ConfigureAwait(false);
            var extensions = new Dictionary<int, VideoNextExtension>();

            foreach (var shard in client.ShardClients.Values)
            {
                extensions.Add(shard.ShardId, shard.GetExtension<VideoNextExtension>());
            }

            return new ReadOnlyDictionary<int, VideoNextExtension>(extensions);
        }

        /// <summary>
        /// Connects to this voice channel using VoiceNext.
        /// </summary>
        /// <param name="channel">Channel to connect to.</param>
        /// <returns>If successful, the VoiceNext connection.</returns>
        public static Task<VideoNextConnection> ConnectAsync(this DiscordChannel channel)
        {
            if (channel == null)
                throw new NullReferenceException();

            if (channel.Guild == null)
                throw new InvalidOperationException("VoiceNext can only be used with guild channels.");

            if (channel.Type != ChannelType.Voice)
                throw new InvalidOperationException("You can only connect to voice channels.");

            if (!(channel.Discord is DiscordClient discord) || discord == null)
                throw new NullReferenceException();

            var vnext = discord.GetVideoNext();
            if (vnext == null)
                throw new InvalidOperationException("VoiceNext is not initialized for this Discord client.");

            var vnc = vnext.GetConnection(channel.Guild);
            if (vnc != null)
                throw new InvalidOperationException("VoiceNext is already connected in this guild.");

            return vnext.ConnectAsync(channel);
        }
    }
}