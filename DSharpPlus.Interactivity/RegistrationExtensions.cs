using System;

using DSharpPlus.Extensions;
using DSharpPlus.Interactivity.Components;
using DSharpPlus.Interactivity.InteractiveMessages;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Interactivity;

/// <summary>
/// Provides methods to register the interactivity extension.
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// Registers interactivity with the given service collection.
    /// </summary>
    /// <param name="services">The service collection to register interactivity into.</param>
    /// <param name="legacy">Legacy features that should be enabled for interactivity.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInteractivityExtension(this IServiceCollection services, LegacyFeatures legacy = LegacyFeatures.None)
    {
        services.AddSingleton<IComponentWaiter, DefaultComponentWaiter>()
            .AddSingleton<IInteractiveMessageHandler, DefaultInteractiveMessageHandler>();

        services.AddTransient<IInteractiveComponentResolver, DefaultInteractiveComponentResolver>();
        
        services.ConfigureEventHandlers(b =>
        {
            b.AddEventHandlers<ComponentEventHandler>(ServiceLifetime.Singleton)
                .AddEventHandlers<InteractiveEventHandler>(ServiceLifetime.Singleton);
        });

        if (legacy == LegacyFeatures.UseReactionInteractivity)
        {
            services.ConfigureEventHandlers(b => b.AddEventHandlers<ReactionHandlerForEwiggestrige>());
            services.Configure<InteractivityOptions>(options => options.UseReactionsForPagination = true);
        }

        if (legacy != LegacyFeatures.None)
        {
            services.Decorate<IInteractiveMessageHandler, PrehistoricInteractiveMessageHandler>();
        }

        return services;
    }

    /// <summary>
    /// Registers interactivity with the given client builder.
    /// </summary>
    /// <param name="builder">The builder to register interactivity into.</param>
    /// <param name="legacy">Legacy features that should be enabled for interactivity.</param>
    /// <returns>The client builder for chaining.</returns>
    public static DiscordClientBuilder UseInteractivity(this DiscordClientBuilder builder, LegacyFeatures legacy = LegacyFeatures.None)
        => builder.ConfigureServices(s => s.AddInteractivityExtension(legacy));

    /// <summary>
    /// Configures interactivity on this client builder.
    /// </summary>
    /// <returns>The client builder for chaining.</returns>
    public static DiscordClientBuilder ConfigureInteractivity(this DiscordClientBuilder builder, Action<InteractivityOptions> configure)
        => builder.ConfigureServices(s => s.Configure(configure));
}
