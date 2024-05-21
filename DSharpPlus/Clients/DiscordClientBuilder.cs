using System;

using DSharpPlus.Extensions;
using DSharpPlus.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus;

/// <summary>
/// Enables building a DiscordClient from complex configuration, registering extensions and fine-tuning behavioural aspects.
/// </summary>
public sealed class DiscordClientBuilder
{
    private readonly IServiceCollection serviceCollection;
    private bool addDefaultLogging = true;

    /// <summary>
    /// Creates a new DiscordClientBuilder from the provided service collection. This is private in favor of static
    /// methods that control creation based on certain presets. IServiceCollection-based configuration occurs separate
    /// from this type.
    /// </summary>
    private DiscordClientBuilder(IServiceCollection serviceCollection)
        => this.serviceCollection = serviceCollection;

    /// <summary>
    /// Creates a new DiscordClientBuilder without sharding, using the specified token.
    /// </summary>
    /// <param name="token">The token to use for this application.</param>
    /// <param name="intents">The intents to connect to the gateway with.</param>
    /// <param name="serviceCollection">The service collection to base this builder on.</param>
    /// <returns>A new DiscordClientBuilder.</returns>
    public static DiscordClientBuilder Default(string token, DiscordIntents intents, IServiceCollection? serviceCollection = null)
    {
        serviceCollection ??= new ServiceCollection();

        DiscordClientBuilder builder = new(serviceCollection);
        builder.serviceCollection.Configure<TokenContainer>(x => x.GetToken = () => token);
        builder.serviceCollection.AddDSharpPlusDefaultsSingleShard(intents);

        return builder;
    }

    /// <summary>
    /// Disables the DSharpPlus default logger for this DiscordClientBuilder.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder DisableDefaultLogging()
    {
        this.addDefaultLogging = false;
        return this;
    }


    /// <summary>
    /// Configures event handlers on the present client builder.
    /// </summary>
    /// <param name="configure">A configuration delegate enabling specific configuration.</param>
    /// <returns>Thecurrent instance for chaining.</returns>
    public DiscordClientBuilder ConfigureEventHandling(Action<EventHandlingBuilder> configure)
    {
        this.serviceCollection.ConfigureEventHandlers(configure);
        return this;
    }

    /// <summary>
    /// Builds a new client from the present builder.
    /// </summary>
    public DiscordClient Build()
    {
        if (this.addDefaultLogging)
        {
            this.serviceCollection.AddLogging(builder => builder.AddProvider(new DefaultLoggerProvider()));
        }

        IServiceProvider provider = this.serviceCollection.BuildServiceProvider();
        return provider.GetRequiredService<DiscordClient>();
    }
}
