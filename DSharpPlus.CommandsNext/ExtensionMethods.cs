// This file is part of DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Development Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
            if (client.GetExtension<CommandsNextExtension>() != null)
                throw new InvalidOperationException("CommandsNext is already enabled for that client.");

            if (!Utilities.HasMessageIntents(client.Configuration.Intents))
                client.Logger.LogCritical(CommandsNextEvents.Intents, "The CommandsNext extension is registered but there are no message intents enabled. It is highly recommended to enable them.");

            if (!client.Configuration.Intents.HasIntent(DiscordIntents.Guilds))
                client.Logger.LogCritical(CommandsNextEvents.Intents, "The CommandsNext extension is registered but the guilds intent is not enabled. It is highly recommended to enable it.");

            var cnext = new CommandsNextExtension(cfg);
            client.AddExtension(cnext);
            return cnext;
        }

        /// <summary>
        /// Enables CommandsNext module on all shards in this <see cref="DiscordShardedClient"/>.
        /// </summary>
        /// <param name="client">Client to enable CommandsNext for.</param>
        /// <param name="cfg">CommandsNext configuration to use.</param>
        /// <returns>A dictionary of created <see cref="CommandsNextExtension"/>, indexed by shard id.</returns>
        public static async Task<IReadOnlyDictionary<int, CommandsNextExtension>> UseCommandsNextAsync(this DiscordShardedClient client, CommandsNextConfiguration cfg)
        {
            var modules = new Dictionary<int, CommandsNextExtension>();
            await client.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                var cnext = shard.GetExtension<CommandsNextExtension>();
                if (cnext == null)
                    cnext = shard.UseCommandsNext(cfg);

                modules[shard.ShardId] = cnext;
            }

            return new ReadOnlyDictionary<int, CommandsNextExtension>(modules);
        }

        /// <summary>
        /// Gets the active CommandsNext module for this client.
        /// </summary>
        /// <param name="client">Client to get CommandsNext module from.</param>
        /// <returns>The module, or null if not activated.</returns>
        public static CommandsNextExtension GetCommandsNext(this DiscordClient client)
            => client.GetExtension<CommandsNextExtension>();


        /// <summary>
        /// Gets the active CommandsNext modules for all shards in this client.
        /// </summary>
        /// <param name="client">Client to get CommandsNext instances from.</param>
        /// <returns>A dictionary of the modules, indexed by shard id.</returns>
        public static async Task<IReadOnlyDictionary<int, CommandsNextExtension>> GetCommandsNextAsync(this DiscordShardedClient client)
        {
            await client.InitializeShardsAsync().ConfigureAwait(false);
            var extensions = new Dictionary<int, CommandsNextExtension>();

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                extensions.Add(shard.ShardId, shard.GetExtension<CommandsNextExtension>());
            }

            return new ReadOnlyDictionary<int, CommandsNextExtension>(extensions);
        }
    }
}
