// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Lavalink;

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

        if (!client.Configuration.Intents.HasIntent(DiscordIntents.GuildVoiceStates))
            client.Logger.LogCritical(LavalinkEvents.Intents, "The Lavalink extension is registered but the guild voice states intent is not enabled. It is highly recommended to enable it.");

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

        if (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage)
            throw new InvalidOperationException("You can only connect to voice and stage channels.");

        if (channel.Discord is not DiscordClient discord || discord == null)
            throw new NullReferenceException();

        var lava = discord.GetLavalink();
        return lava == null
            ? throw new InvalidOperationException("Lavalink is not initialized for this Discord client.")
            : node.ConnectAsync(channel);
    }
}
