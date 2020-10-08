﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Lavalink
{
    public static class DiscordClientExtensions
    {
        /// <summary>
        /// Creates a new Lavalink client with specified settings.
        /// </summary>
        /// <param name="client">Discord client to create Lavalink instance for.</param>
        /// <returns>Lavalink client instance.</returns>
        public static LavalinkExtension UseLavalink(this DiscordClient client)
        {
            if (client.GetExtension<LavalinkExtension>() != null)
                throw new InvalidOperationException("Lavalink is already enabled for that client.");

            if (client.Configuration.Intents.HasValue)
            {
                if (!client.Configuration.Intents.Value.HasIntent(DiscordIntents.GuildVoiceStates))
                    client.Logger.LogCritical(LavalinkEvents.Intents, "The Lavalink extension is registered but the guild voice states intent is not enabled. It is highly recommended to enable it.");
            }

            var lava = new LavalinkExtension();
            client.AddExtension(lava);
            return lava;
        }

        /// <summary>
        /// Creates new Lavalink clients on all shards in a given sharded client.
        /// </summary>
        /// <param name="client">Discord sharded client to create Lavalink instances for.</param>
        /// <returns>A dictionary of created Lavalink clients.</returns>
        public static async Task<IReadOnlyDictionary<int, LavalinkExtension>> UseLavalinkAsync(this DiscordShardedClient client)
        {
            var modules = new Dictionary<int, LavalinkExtension>();
            await client.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                var lava = shard.GetExtension<LavalinkExtension>();
                if (lava == null)
                    lava = shard.UseLavalink();

                modules[shard.ShardId] = lava;
            }

            return new ReadOnlyDictionary<int, LavalinkExtension>(modules);
        }

        /// <summary>
        /// Gets the active instance of the Lavalink client for the DiscordClient.
        /// </summary>
        /// <param name="client">Discord client to get Lavalink instance for.</param>
        /// <returns>Lavalink client instance.</returns>
        public static LavalinkExtension GetLavalink(this DiscordClient client)
            => client.GetExtension<LavalinkExtension>();

        /// <summary>
        /// Retrieves a <see cref="LavalinkExtension"/> instance for each shard.
        /// </summary>
        /// <param name="client">The shard client to retrieve <see cref="LavalinkExtension"/> instances from.</param>
        /// <returns>A dictionary containing <see cref="LavalinkExtension"/> instances for each shard.</returns>
        public static async Task<IReadOnlyDictionary<int, LavalinkExtension>> GetLavalinkAsync(this DiscordShardedClient client)
        {
            await client.InitializeShardsAsync().ConfigureAwait(false);
            var extensions = new Dictionary<int, LavalinkExtension>();

            foreach (var shard in client.ShardClients.Values)
            {
                extensions.Add(shard.ShardId, shard.GetExtension<LavalinkExtension>());
            }

            return new ReadOnlyDictionary<int, LavalinkExtension>(extensions);
        }

        /// <summary>
        /// Connects to this voice channel using Lavalink.
        /// </summary>
        /// <param name="channel">Channel to connect to.</param>
        /// <param name="node">Lavalink node to connect through.</param>
        /// <returns>If successful, the Lavalink client.</returns>
        public static Task ConnectAsync(this DiscordChannel channel, LavalinkNodeConnection node)
        {
            if (channel == null)
                throw new NullReferenceException();

            if (channel.Guild == null)
                throw new InvalidOperationException("Lavalink can only be used with guild channels.");

            if (channel.Type != ChannelType.Voice)
                throw new InvalidOperationException("You can only connect to voice channels.");

            if (!(channel.Discord is DiscordClient discord) || discord == null)
                throw new NullReferenceException();

            var lava = discord.GetLavalink();
            if (lava == null)
                throw new InvalidOperationException("Lavalink is not initialized for this Discord client.");

            return node.ConnectAsync(channel);
        }
    }
}
