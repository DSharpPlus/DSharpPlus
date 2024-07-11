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

        CommandsExtension extension = new(configuration ?? new());
        client.AddExtension(extension);
        return extension;
    }

    /// <summary>
    /// Retrieves the <see cref="CommandsExtension"/> from the <see cref="DiscordClient"/>.
    /// </summary>
    /// <param name="client">The client to retrieve the extension from.</param>
    public static CommandsExtension? GetCommandsExtension(this DiscordClient client) => client is null
        ? throw new ArgumentNullException(nameof(client))
        : client.GetExtension<CommandsExtension>();

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
}
