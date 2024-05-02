
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands;
/// <summary>
/// Extension methods used by the <see cref="CommandsExtension"/> for the <see cref="DiscordClient"/>.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Registers the extension with the <see cref="DiscordClient"/>.
    /// </summary>
    /// <param name="client">The client to register the extension with.</param>
    /// <param name="configuration">The configuration to use for the extension.</param>
    public static CommandsExtension UseCommands(this DiscordClient client, CommandsConfiguration? configuration = null)
    {
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }
        else if (client.GetExtension<CommandsExtension>() is not null)
        {
            throw new InvalidOperationException("Commands extension is already initialized.");
        }

        CommandsExtension extension = new(configuration ?? GrabDefaultConfiguration(client.Logger));
        client.AddExtension(extension);
        return extension;
    }

    /// <summary>
    /// Registers the extension with all the shards on the <see cref="DiscordShardedClient"/>.
    /// </summary>
    /// <param name="shardedClient">The client to register the extension with.</param>
    /// <param name="configuration">The configuration to use for the extension.</param>
    public static async Task<IReadOnlyDictionary<int, CommandsExtension>> UseCommandsAsync(this DiscordShardedClient shardedClient, CommandsConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(shardedClient);

        await shardedClient.InitializeShardsAsync();
        configuration ??= GrabDefaultConfiguration(shardedClient.Logger);

        Dictionary<int, CommandsExtension> extensions = [];
        foreach (DiscordClient shard in shardedClient.ShardClients.Values)
        {
            extensions[shard.ShardId] = shard.GetExtension<CommandsExtension>() ?? shard.UseCommands(configuration);
        }

        return extensions.AsReadOnly();
    }

    /// <summary>
    /// Retrieves the <see cref="CommandsExtension"/> from the <see cref="DiscordClient"/>.
    /// </summary>
    /// <param name="client">The client to retrieve the extension from.</param>
    public static CommandsExtension? GetCommandsExtension(this DiscordClient client) => client is null
        ? throw new ArgumentNullException(nameof(client))
        : client.GetExtension<CommandsExtension>();

    /// <summary>
    /// Retrieves the <see cref="CommandsExtension"/> from all of the shards on <see cref="DiscordShardedClient"/>.
    /// </summary>
    /// <param name="shardedClient">The client to retrieve the extension from.</param>
    public static IReadOnlyDictionary<int, CommandsExtension> GetCommandsExtensions(this DiscordShardedClient shardedClient)
    {
        ArgumentNullException.ThrowIfNull(shardedClient);

        Dictionary<int, CommandsExtension> extensions = [];
        foreach (DiscordClient shard in shardedClient.ShardClients.Values)
        {
            CommandsExtension? extension = shard.GetExtension<CommandsExtension>();
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

    private static CommandsConfiguration GrabDefaultConfiguration(ILogger logger)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging(loggerBuilder =>
        {
            logger.LogError("Commands: No logger was provided, using NullLoggerProvider. This is not recommended.");
            loggerBuilder.AddProvider(NullLoggerProvider.Instance);
        });

        return new()
        {
            ServiceProvider = services.BuildServiceProvider()
        };
    }
}
