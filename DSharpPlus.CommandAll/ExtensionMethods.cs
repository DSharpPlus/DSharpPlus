namespace DSharpPlus.CommandAll;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Extension methods used by the <see cref="CommandAllExtension"/> for the <see cref="DiscordClient"/>.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Registers the extension with the <see cref="DiscordClient"/>.
    /// </summary>
    /// <param name="client">The client to register the extension with.</param>
    /// <param name="configuration">The configuration to use for the extension.</param>
    public static CommandAllExtension UseCommandAll(this DiscordClient client, CommandAllConfiguration? configuration = null)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }
        else if (client.GetExtension<CommandAllExtension>() is not null)
        {
            throw new InvalidOperationException("CommandAll Extension is already initialized.");
        }

        CommandAllExtension extension = new(configuration ?? GrabDefaultConfiguration(client.Logger));
        client.AddExtension(extension);
        return extension;
    }

    /// <summary>
    /// Registers the extension with all the shards on the <see cref="DiscordShardedClient"/>.
    /// </summary>
    /// <param name="shardedClient">The client to register the extension with.</param>
    /// <param name="configuration">The configuration to use for the extension.</param>
    public static async Task<IReadOnlyDictionary<int, CommandAllExtension>> UseCommandAllAsync(this DiscordShardedClient shardedClient, CommandAllConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(shardedClient);

        await shardedClient.InitializeShardsAsync();
        configuration ??= GrabDefaultConfiguration(shardedClient.Logger);

        Dictionary<int, CommandAllExtension> extensions = [];
        foreach (DiscordClient shard in shardedClient.ShardClients.Values)
        {
            extensions[shard.ShardId] = shard.GetExtension<CommandAllExtension>() ?? shard.UseCommandAll(configuration);
        }

        return extensions.AsReadOnly();
    }

    /// <summary>
    /// Retrieves the <see cref="CommandAllExtension"/> from the <see cref="DiscordClient"/>.
    /// </summary>
    /// <param name="client">The client to retrieve the extension from.</param>
    public static CommandAllExtension? GetCommandAllExtension(this DiscordClient client) => client is null
        ? throw new ArgumentNullException(nameof(client))
        : client.GetExtension<CommandAllExtension>();

    /// <summary>
    /// Retrieves the <see cref="CommandAllExtension"/> from all of the shards on <see cref="DiscordShardedClient"/>.
    /// </summary>
    /// <param name="shardedClient">The client to retrieve the extension from.</param>
    public static IReadOnlyDictionary<int, CommandAllExtension> GetCommandAllExtensions(this DiscordShardedClient shardedClient)
    {
        ArgumentNullException.ThrowIfNull(shardedClient);

        Dictionary<int, CommandAllExtension> extensions = [];
        foreach (DiscordClient shard in shardedClient.ShardClients.Values)
        {
            CommandAllExtension? extension = shard.GetExtension<CommandAllExtension>();
            if (extension is not null)
            {
                extensions[shard.ShardId] = extension;
            }
        }

        return extensions.AsReadOnly();
    }

    /// <inheritdoc cref="Array.IndexOf{T}(T[], T)"/>
    internal static int IndexOf<T>(this IEnumerable<T> array, T? value) where T : IEquatable<T>
    {
        int index = 0;
        foreach (T item in array)
        {
            if (item.Equals(value))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private static CommandAllConfiguration GrabDefaultConfiguration(ILogger logger)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging(loggerBuilder =>
        {
            logger.LogWarning("CommandAll: No logger was provided, using NullLoggerProvider. This is not recommended.");
            loggerBuilder.AddProvider(NullLoggerProvider.Instance);
        });

        return new()
        {
            ServiceProvider = services.BuildServiceProvider()
        };
    }
}
