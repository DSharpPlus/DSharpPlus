using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.Extensions;

/// <summary>
/// Interactivity extension methods for <see cref="DiscordClient"/> and <see cref="DiscordShardedClient"/>.
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Enables interactivity for this <see cref="DiscordClient"/> instance.
    /// </summary>
    /// <param name="client">The client to enable interactivity for.</param>
    /// <param name="configuration">A configuration instance. Default configuration values will be used if none is provided.</param>
    /// <returns>A brand new <see cref="InteractivityExtension"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if interactivity has already been enabled for the client instance.</exception>
    public static InteractivityExtension UseInteractivity(this DiscordClient client, InteractivityConfiguration configuration = null)
    {
        if (client.GetExtension<InteractivityExtension>() != null)
        {
            throw new InvalidOperationException($"Interactivity is already enabled for this {(client._isShard ? "shard" : "client")}.");
        }

        configuration ??= new InteractivityConfiguration();
        InteractivityExtension extension = new InteractivityExtension(configuration);
        client.AddExtension(extension);

        return extension;
    }

    /// <summary>
    /// Enables interactivity for each shard.
    /// </summary>
    /// <param name="client">The shard client to enable interactivity for.</param>
    /// <param name="configuration">Configuration to use for all shards. If one isn't provided, default configuration values will be used.</param>
    /// <returns>A dictionary containing new <see cref="InteractivityExtension"/> instances for each shard.</returns>
    public static async Task<IReadOnlyDictionary<int, InteractivityExtension>> UseInteractivityAsync(this DiscordShardedClient client, InteractivityConfiguration configuration = null)
    {
        Dictionary<int, InteractivityExtension> extensions = new Dictionary<int, InteractivityExtension>();
        await client.InitializeShardsAsync();

        foreach (DiscordClient? shard in client.ShardClients.Select(xkvp => xkvp.Value))
        {
            InteractivityExtension extension = shard.GetExtension<InteractivityExtension>() ?? shard.UseInteractivity(configuration);
            extensions.Add(shard.ShardId, extension);
        }

        return new ReadOnlyDictionary<int, InteractivityExtension>(extensions);
    }

    /// <summary>
    /// Retrieves the registered <see cref="InteractivityExtension"/> instance for this client.
    /// </summary>
    /// <param name="client">The client to retrieve an <see cref="InteractivityExtension"/> instance from.</param>
    /// <returns>An existing <see cref="InteractivityExtension"/> instance, or <see langword="null"/> if interactivity is not enabled for the <see cref="DiscordClient"/> instance.</returns>
    public static InteractivityExtension GetInteractivity(this DiscordClient client)
        => client.GetExtension<InteractivityExtension>();

    /// <summary>
    /// Retrieves a <see cref="InteractivityExtension"/> instance for each shard.
    /// </summary>
    /// <param name="client">The shard client to retrieve interactivity instances from.</param>
    /// <returns>A dictionary containing <see cref="InteractivityExtension"/> instances for each shard.</returns>
    public static async Task<ReadOnlyDictionary<int, InteractivityExtension>> GetInteractivityAsync(this DiscordShardedClient client)
    {
        await client.InitializeShardsAsync();
        Dictionary<int, InteractivityExtension> extensions = new Dictionary<int, InteractivityExtension>();

        foreach (DiscordClient? shard in client.ShardClients.Select(xkvp => xkvp.Value))
        {
            extensions.Add(shard.ShardId, shard.GetExtension<InteractivityExtension>());
        }

        return new ReadOnlyDictionary<int, InteractivityExtension>(extensions);
    }
}
