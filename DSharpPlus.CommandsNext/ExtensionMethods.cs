using System;

using DSharpPlus.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandsNext;

/// <summary>
/// Defines various extensions specific to CommandsNext.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Adds the CommandsNext extension to this DiscordClientBuilder.
    /// </summary>
    /// <param name="builder">The builder to register to.</param>
    /// <param name="setup">Any setup code you want to run on the extension, such as registering commands and converters.</param>
    /// <param name="configuration">CommandsNext configuration to use.</param>
    /// <returns>The same builder for chaining.</returns>
    public static DiscordClientBuilder UseCommandsNext
    (
        this DiscordClientBuilder builder,
        Action<CommandsNextExtension> setup,
        CommandsNextConfiguration configuration
    )
        => builder.ConfigureServices(services => services.AddCommandsNextExtension(setup, configuration));

    /// <summary>
    /// Adds the CommandsNext extension to this service collection.
    /// </summary>
    /// <param name="services">The service collection to register to.</param>
    /// <param name="setup">Any setup code you want to run on the extension, such as registering commands and converters.</param>
    /// <param name="configuration">CommandsNext configuration to use.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddCommandsNextExtension
    (
        this IServiceCollection services,
        Action<CommandsNextExtension> setup,
        CommandsNextConfiguration configuration
    )
    {
        if (configuration.UseDefaultCommandHandler)
        {
            services.ConfigureEventHandlers(b => b.AddEventHandlers<MessageHandler>());
        }

        services.AddSingleton(provider =>
        {
            DiscordClient client = provider.GetRequiredService<DiscordClient>();

            CommandsNextExtension extension = new(configuration ?? new());
            extension.Setup(client);
            setup(extension);

            return extension;
        });

        return services;
    }
}
