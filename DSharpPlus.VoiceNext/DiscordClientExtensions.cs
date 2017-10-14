using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext
{
    public static class DiscordClientExtensions
    {
        /// <summary>
        /// Creates a new VoiceNext client with default settings.
        /// </summary>
        /// <param name="client">Discord client to create VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextExtension UseVoiceNext(this DiscordClient client) 
            => UseVoiceNext(client, new VoiceNextConfiguration { VoiceApplication = VoiceApplication.Music });

        /// <summary>
        /// Creates a new VoiceNext client with specified settings.
        /// </summary>
        /// <param name="client">Discord client to create VoiceNext instance for.</param>
        /// <param name="config">Configuration for the VoiceNext client.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextExtension UseVoiceNext(this DiscordClient client, VoiceNextConfiguration config)
        {
            if (client.GetExtension<VoiceNextExtension>() != null)
                throw new InvalidOperationException("VoiceNext is already enabled for that client.");

            var vnext = new VoiceNextExtension(config);
            client.AddExtension(vnext);
            return vnext;
        }

        /// <summary>
        /// Creates new VoiceNext clients on all shards in a given sharded client.
        /// </summary>
        /// <param name="client">Discord sharded client to create VoiceNext instances for.</param>
        /// <param name="config">Configuration for the VoiceNext clients.</param>
        /// <returns>A dictionary of created VoiceNext clients.</returns>
        public static IReadOnlyDictionary<int, VoiceNextExtension> UseVoiceNext(this DiscordShardedClient client, VoiceNextConfiguration config)
        {
            var modules = new Dictionary<int, VoiceNextExtension>();

            client.InitializeShardsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                var cnext = shard.GetExtension<VoiceNextExtension>();
                if (cnext == null)
                    cnext = shard.UseVoiceNext(config);

                modules.Add(shard.ShardId, cnext);
            }

            return new ReadOnlyDictionary<int, VoiceNextExtension>(modules);
        }

        /// <summary>
        /// Gets the active instance of VoiceNext client for the DiscordClient.
        /// </summary>
        /// <param name="client">Discord client to get VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextExtension GetVoiceNextClient(this DiscordClient client) 
            => client.GetExtension<VoiceNextExtension>();
    }
}
