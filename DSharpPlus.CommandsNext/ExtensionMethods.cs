﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Defines various extensions specific to CommandsNext.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Enables CommandsNext module on this <see cref="DiscordClient"/>.
        /// </summary>
        /// <param name="client">Client to enable CommandsNext for.</param>
        /// <param name="cfg">CommandsNext configuration to use.</param>
        /// <returns>Created <see cref="CommandsNextExtension"/>.</returns>
        public static CommandsNextExtension UseCommandsNext(this DiscordClient client, CommandsNextConfiguration cfg)
        {
            if (client.GetModule<CommandsNextExtension>() != null)
            {
                throw new InvalidOperationException("CommandsNext is already enabled for that client.");
            }

            var cnext = new CommandsNextExtension(cfg);
            client.AddModule(cnext);
            return cnext;
        }

        /// <summary>
        /// Enables CommandsNext module on all shards in this <see cref="DiscordShardedClient"/>.
        /// </summary>
        /// <param name="client">Client to enable CommandsNext for.</param>
        /// <param name="cfg">CommandsNext configuration to use.</param>
        /// <returns>A dictionary of created <see cref="CommandsNextExtension"/>, indexed by shard id..</returns>
        public static IReadOnlyDictionary<int, CommandsNextExtension> UseCommandsNext(this DiscordShardedClient client, CommandsNextConfiguration cfg)
        {
            var modules = new Dictionary<int, CommandsNextExtension>();

            client.InitializeShardsAsync().GetAwaiter().GetResult();

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                var cnext = shard.GetModule<CommandsNextExtension>();
                if (cnext == null)
                {
                    cnext = shard.UseCommandsNext(cfg);
                }

                modules.Add(shard.ShardId, cnext);
            }

            return new ReadOnlyDictionary<int, CommandsNextExtension>(modules);
        }

        /// <summary>
        /// Gets the active CommandsNext module for this client.
        /// </summary>
        /// <param name="client">Client to get CommandsNext module from.</param>
        /// <returns>The module, or null if not activated.</returns>
        public static CommandsNextExtension GetCommandsNext(this DiscordClient client)
        {
            return client.GetModule<CommandsNextExtension>();
        }

        /// <summary>
        /// Gets the active CommandsNext modules for all shards in this client.
        /// </summary>
        /// <param name="client">Client to get CommandsNext instances from.</param>
        /// <returns>A dictionary of the modules, indexed by shard id.</returns>
        public static IReadOnlyDictionary<int, CommandsNextExtension> GetCommandsNext(this DiscordShardedClient client)
        {
            var modules = new Dictionary<int, CommandsNextExtension>();

            client.InitializeShardsAsync().GetAwaiter().GetResult();

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                modules.Add(shard.ShardId, shard.GetModule<CommandsNextExtension>());
            }

            return new ReadOnlyDictionary<int, CommandsNextExtension>(modules);
        }
    }
}
