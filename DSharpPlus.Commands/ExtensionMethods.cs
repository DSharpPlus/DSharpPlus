using System;
using System.Collections.Generic;
using DSharpPlus.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Commands;

/// <summary>
/// Extension methods used by the <see cref="CommandsExtension"/> for the <see cref="DiscordClient"/>.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Registers the extension with the <see cref="DiscordClientBuilder"/>.
    /// </summary>
    /// <param name="builder">The client builder to register the extension with.</param>
    /// <param name="setup">Any setup code you want to run on the extension, such as registering commands and converters.</param>
    /// <param name="configuration">The configuration to use for the extension.</param>
    public static DiscordClientBuilder UseCommands(this DiscordClientBuilder builder, Action<IServiceProvider, CommandsExtension> setup, CommandsConfiguration? configuration = null)
        => builder.ConfigureServices(services => services.AddCommandsExtension(setup, configuration));

    /// <summary>
    /// Registers the commands extension with an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register the extension with.</param>
    /// <param name="setup">Any setup code you want to run on the extension, such as registering commands and converters.</param>
    /// <param name="configuration">The configuration to use for the extension.</param>
    public static IServiceCollection AddCommandsExtension(this IServiceCollection services, Action<IServiceProvider, CommandsExtension> setup, CommandsConfiguration? configuration = null)
    {
        AddCommandsExtension(services, setup, _ => configuration ?? new CommandsConfiguration());
        return services;
    }

    /// <inheritdoc cref="AddCommandsExtension(IServiceCollection, Action{IServiceProvider, CommandsExtension}, CommandsConfiguration?)"/>
    public static IServiceCollection AddCommandsExtension(this IServiceCollection services, Action<IServiceProvider, CommandsExtension> setup, Func<IServiceProvider, CommandsConfiguration> configurationFactory)
    {
        services
            .ConfigureEventHandlers(eventHandlingBuilder => eventHandlingBuilder
                .AddEventHandlers<RefreshEventHandler>(ServiceLifetime.Singleton)
                .AddEventHandlers<ProcessorInvokingHandlers>(ServiceLifetime.Transient)
            )
            .AddSingleton(provider =>
            {
                DiscordClient client = provider.GetRequiredService<DiscordClient>();
                CommandsConfiguration configuration = configurationFactory(provider);
                CommandsExtension extension = new(configuration);
                extension.Setup(client);
                setup(provider, extension);
                return extension;
            });

        return services;
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
}
