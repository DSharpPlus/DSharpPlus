using System;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Extensions;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures event handlers on the present service collection.
    /// </summary>
    /// <param name="services">The service collection to add event handlers to.</param>
    /// <param name="configure">A configuration delegate enabling specific configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureEventHandlers
    (
        this IServiceCollection services,
        Action<EventHandlingBuilder> configure
    )
    {
        EventHandlingBuilder builder = new(services);

        configure(builder);

        return services;
    }
}
