using DSharpPlus.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Interactivity.Extensions;

/// <summary>
/// Interactivity extension methods for <see cref="DiscordClient"/>.
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Enables interactivity for this <see cref="DiscordClientBuilder"/> instance.
    /// </summary>
    /// <param name="builder">The client builder to enable interactivity for.</param>
    /// <param name="configuration">A configuration instance. Default configuration values will be used if none is provided.</param>
    /// <returns>The client builder for chaining.</returns>
    public static DiscordClientBuilder UseInteractivity
    (
        this DiscordClientBuilder builder,
        InteractivityConfiguration? configuration = null
    )
    {
        builder.ConfigureServices(services => services.AddInteractivityExtension(configuration));

        return builder;
    }

    /// <summary>
    /// Adds interactivity to the present service collection.
    /// </summary>
    /// <param name="services">The service collection to enable interactivity for.</param>
    /// <param name="configuration">A configuration instance. Default configuration values will be used if none is provided.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInteractivityExtension
    (
        this IServiceCollection services,
        InteractivityConfiguration? configuration = null
    )
    {
        services.ConfigureEventHandlers(b => b.AddEventHandlers<InteractivityEventHandler>(ServiceLifetime.Transient))
            .AddSingleton(provider =>
            {
                DiscordClient client = provider.GetRequiredService<DiscordClient>();

                InteractivityExtension extension = new(configuration ?? new());
                extension.Setup(client);

                return extension;
            });

        return services;
    }

    internal static InteractivityExtension GetInteractivity(this DiscordClient client)
        => client.ServiceProvider.GetRequiredService<InteractivityExtension>();
}
