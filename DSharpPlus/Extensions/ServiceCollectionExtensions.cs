using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Extensions;

/// <summary>
/// Provides extension methods on <see cref="IServiceCollection"/>.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DSharpPlus' DiscordClient and all its dependent services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the DiscordClient to.</param>
    /// <param name="token">The bot token to use to connect to Discord.</param>
    /// <param name="intents">The intents to use to connect to Discord.</param>
    /// <returns>The current instance for chaining.</returns>
    public static IServiceCollection AddDiscordClient
    (
        this IServiceCollection services,
        string token,
        DiscordIntents intents
    )
    {
        services.Configure<TokenContainer>(c => c.GetToken = () => token);
        services.AddDSharpPlusDefaultsSingleShard(intents);
        return services;
    }

    /// <summary>
	/// Decorates a given <typeparamref name="TInterface"/> with a decorator of type <typeparamref name="TDecorator"/>.
	/// </summary>
	/// <typeparam name="TInterface">
    /// The interface type to be decorated. The newly registered decorator can be decorated again if needed.
    /// </typeparam>
	/// <typeparam name="TDecorator">The decorator type. This type may be decorated again.</typeparam>
	/// <returns>The service collection for chaining.</returns>
	/// <exception cref="InvalidOperationException">
    /// Thrown if this method is called before a service of type <typeparamref name="TInterface"/> was registered.
    /// </exception>
	public static IServiceCollection Decorate<TInterface, TDecorator>(this IServiceCollection services)
        where TInterface : class
        where TDecorator : class, TInterface
    {
        ServiceDescriptor? previousRegistration = services.LastOrDefault(xm => xm.ServiceType == typeof(TInterface))
            ?? throw new InvalidOperationException
            (
                $"Tried to register a decorator for {typeof(TInterface).Name}, but there was no underlying service to decorate."
            );

        Func<IServiceProvider, object>? previousFactory = previousRegistration.ImplementationFactory;

        if (previousFactory is null && previousRegistration.ImplementationInstance is not null)
        {
            previousFactory = _ => previousRegistration.ImplementationInstance;
        }
        else if (previousFactory is null && previousRegistration.ImplementationType is not null)
        {
            previousFactory = provider => ActivatorUtilities.CreateInstance
            (
                provider,
                previousRegistration.ImplementationType
            );
        }

        services.Add
        (
            new ServiceDescriptor
            (
                typeof(TInterface),
                CreateDecorator,
                previousRegistration.Lifetime
            )
        );

        return services;

        TDecorator CreateDecorator(IServiceProvider provider)
        {
            TInterface previousInstance = (TInterface)previousFactory!(provider);

            TDecorator decorator = (TDecorator)ActivatorUtilities.CreateFactory
            (
                typeof(TDecorator),
                [
                    typeof(TInterface)
                ]
            )
            .Invoke(provider, [previousInstance]);

            return decorator;
        }
    }
}
