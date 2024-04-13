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
    [Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
    public static LavalinkExtension UseLavalink(this DiscordClient client)
    {
        if (client.GetExtension<LavalinkExtension>() != null)
        {
            throw new InvalidOperationException("Lavalink is already enabled for that client.");
        }

        if (!client.Configuration.Intents.HasIntent(DiscordIntents.GuildVoiceStates))
        {
            client.Logger.LogCritical(LavalinkEvents.Intents, "The Lavalink extension is registered but the guild voice states intent is not enabled. It is highly recommended to enable it.");
        }

        LavalinkExtension lava = new LavalinkExtension();
        client.AddExtension(lava);
        return lava;
    }

    /// <summary>
    /// Creates new Lavalink clients on all shards in a given sharded client.
    /// </summary>
    /// <param name="client">Discord sharded client to create Lavalink instances for.</param>
    /// <returns>A dictionary of created Lavalink clients.</returns>

    [Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
    public static async Task<IReadOnlyDictionary<int, LavalinkExtension>> UseLavalinkAsync(this DiscordShardedClient client)
    {
        Dictionary<int, LavalinkExtension> modules = new Dictionary<int, LavalinkExtension>();
        await client.InitializeShardsAsync();

        foreach (DiscordClient? shard in client.ShardClients.Select(xkvp => xkvp.Value))
        {
            LavalinkExtension? lava = shard.GetExtension<LavalinkExtension>() ?? shard.UseLavalink();
            modules[shard.ShardId] = lava;
        }

        return new ReadOnlyDictionary<int, LavalinkExtension>(modules);
    }

    /// <summary>
    /// Gets the active instance of the Lavalink client for the DiscordClient.
    /// </summary>
    /// <param name="client">Discord client to get Lavalink instance for.</param>
    /// <returns>Lavalink client instance.</returns>
    [Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
    public static LavalinkExtension GetLavalink(this DiscordClient client)
        => client.GetExtension<LavalinkExtension>();

    /// <summary>
    /// Retrieves a <see cref="LavalinkExtension"/> instance for each shard.
    /// </summary>
    /// <param name="client">The shard client to retrieve <see cref="LavalinkExtension"/> instances from.</param>
    /// <returns>A dictionary containing <see cref="LavalinkExtension"/> instances for each shard.</returns>
    [Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
    public static async Task<IReadOnlyDictionary<int, LavalinkExtension>> GetLavalinkAsync(this DiscordShardedClient client)
    {
        await client.InitializeShardsAsync();
        Dictionary<int, LavalinkExtension> extensions = new Dictionary<int, LavalinkExtension>();

        foreach (DiscordClient shard in client.ShardClients.Values)
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
    [Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
    public static Task ConnectAsync(this DiscordChannel channel, LavalinkNodeConnection node)
    {
        if (channel == null)
        {
            throw new NullReferenceException();
        }

        if (channel.Guild == null)
        {
            throw new InvalidOperationException("Lavalink can only be used with guild channels.");
        }

        if (channel.Type != DiscordChannelType.Voice && channel.Type != DiscordChannelType.Stage)
        {
            throw new InvalidOperationException("You can only connect to voice and stage channels.");
        }

        if (channel.Discord is not DiscordClient discord || discord == null)
        {
            throw new NullReferenceException();
        }

        LavalinkExtension lava = discord.GetLavalink();
        return lava == null
            ? throw new InvalidOperationException("Lavalink is not initialized for this Discord client.")
            : node.ConnectAsync(channel);
    }
}
