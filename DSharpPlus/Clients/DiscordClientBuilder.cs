using System;
using System.Threading.Tasks;

using DSharpPlus.Clients;
using DSharpPlus.Extensions;
using DSharpPlus.Logging;
using DSharpPlus.Net;
using DSharpPlus.Net.Gateway;
using DSharpPlus.Net.Gateway.Compression;
using DSharpPlus.Net.Gateway.Compression.Zlib;
using DSharpPlus.Net.Gateway.Compression.Zstd;

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
    private bool sharding = false;
    private LogLevel minimumLogLevel = LogLevel.Information;

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
    public static DiscordClientBuilder CreateDefault
    (
        string token,
        DiscordIntents intents,
        IServiceCollection? serviceCollection = null
    )
    {
        serviceCollection ??= new ServiceCollection();

        DiscordClientBuilder builder = new(serviceCollection);
        builder.serviceCollection.Configure<TokenContainer>(x => x.GetToken = () => token);
        builder.serviceCollection.AddDSharpPlusDefaultsSingleShard(intents);

        return builder;
    }

    /// <summary>
    /// Creates a new sharding DiscordClientbuilder using the specified token.
    /// </summary>
    /// <remarks>
    /// DSharpPlus supports more advanced sharding over just specifying the amount of shards, which can be accessed
    /// through the underlying service collection: <br/>
    /// <code>
    /// builder.ConfigureServices(services =>
    /// {
    ///     services.Configure&lt;ShardingOptions&gt;(x => ...);
    ///     
    ///     // The default orchestrator supports a shard count and a "stride" (offset from shard 0), which requires
    ///     // a total shard count. If you wish to customize sharding further, you can specify your own orchestrator:
    ///     services.AddSingleton&lt;IShardOrchestrator, MyCustomShardOrchestrator&gt;();
    /// }
    /// </code>
    /// </remarks>
    /// <param name="token">The token to use for this application.</param>
    /// <param name="intents">The intents to connect to the gateway with.</param>
    /// <param name="shardCount">The amount of shards to start.</param>
    /// <param name="serviceCollection">The service collection to base this builder on.</param>
    /// <returns>A new DiscordClientBuilder.</returns>
    public static DiscordClientBuilder CreateSharded
    (
        string token,
        DiscordIntents intents,
        uint? shardCount = null,
        IServiceCollection? serviceCollection = null
    )
    {
        serviceCollection ??= new ServiceCollection();

        DiscordClientBuilder builder = new(serviceCollection);
        builder.serviceCollection.Configure<ShardingOptions>(x => x.ShardCount = shardCount);
        builder.serviceCollection.Configure<TokenContainer>(x => x.GetToken = () => token);
        builder.serviceCollection.AddDSharpPlusDefaultsSharded(intents);

        builder.sharding = true;

        return builder;
    }

    /// <summary>
    /// Sets the gateway compression used to zstd. This requires zstd natives to be available to the application.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder UseZstdCompression()
    {
        this.serviceCollection.Replace<IPayloadDecompressor, ZstdDecompressor>();
        return this;
    }

    /// <summary>
    /// Sets the gateway compression used to zlib. This is the default compression mode.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder UseZlibCompression()
    {
        this.serviceCollection.Replace<IPayloadDecompressor, ZlibStreamDecompressor>();
        return this;
    }

    /// <summary>
    /// Disables gateway compression entirely.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder DisableGatewayCompression()
    {
        this.serviceCollection.Replace<IPayloadDecompressor, NullDecompressor>();
        return this;
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
    /// Sets the log level for the default logger, should it be used.
    /// </summary>
    /// <remarks>
    /// This does not affect custom logging configurations.
    /// </remarks>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder SetLogLevel(LogLevel minimum)
    {
        this.minimumLogLevel = minimum;
        return this;
    }

    /// <summary>
    /// Configures logging for this DiscordClientBuilder and disables the default DSharpPlus logger.
    /// </summary>
    /// <param name="configure">The configuration delegate.</param>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
    {
        this.addDefaultLogging = false;
        this.serviceCollection.AddLogging(configure);
        return this;
    }

    /// <summary>
    /// Configures services on this DiscordClientBuilder, enabling you to customize the library or your own services.
    /// </summary>
    /// <param name="configure">The configuration delegate.</param>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        configure(this.serviceCollection);
        return this;
    }

    /// <summary>
    /// Configures event handlers on the present client builder.
    /// </summary>
    /// <param name="configure">A configuration delegate enabling specific configuration.</param>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder ConfigureEventHandlers(Action<EventHandlingBuilder> configure)
    {
        this.serviceCollection.ConfigureEventHandlers(configure);
        return this;
    }

    /// <summary>
    /// Configures the rest client used by DSharpPlus.
    /// </summary>
    /// <param name="configure">A configuration delegate for the rest client.</param>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder ConfigureRestClient(Action<RestClientOptions> configure)
    {
        this.serviceCollection.Configure(configure);
        return this;
    }

    /// <summary>
    /// Configures the gateway client used by DSharpPlus.
    /// </summary>
    /// <param name="configure">A configuration delegate for the gateway client.</param>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder ConfigureGatewayClient(Action<GatewayClientOptions> configure)
    {
        this.serviceCollection.Configure(configure);
        return this;
    }

    /// <summary>
    /// Configures the sharding attempted by DSharpPlus. Throws if the builder was not set up for sharding.
    /// </summary>
    /// <param name="configure">A configuration delegate for sharding.</param>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder ConfigureSharding(Action<ShardingOptions> configure)
    {
        if (!this.sharding)
        {
            throw new InvalidOperationException("This client builder is not set up for sharding.");
        }

        this.serviceCollection.Configure(configure);
        return this;
    }

    /// <summary>
    /// Tweaks assorted extra configuration knobs around the library.
    /// </summary>
    /// <param name="configure">A configuration delegate for the remaining library.</param>
    /// <returns>The current instance for chaining.</returns>
    public DiscordClientBuilder ConfigureExtraFeatures(Action<DiscordConfiguration> configure)
    {
        this.serviceCollection.Configure(configure);
        return this;
    }

    /// <summary>
    /// Builds a new client from the present builder.
    /// </summary>
    public DiscordClient Build()
    {
        if (this.addDefaultLogging)
        {
            this.serviceCollection.AddLogging(builder => builder.AddProvider(new DefaultLoggerProvider(this.minimumLogLevel)));
        }

        IServiceProvider provider = this.serviceCollection.BuildServiceProvider();
        return provider.GetRequiredService<DiscordClient>();
    }

    /// <summary>
    /// Builds the client and connects to Discord. The client instance will be unobtainable in user code.
    /// </summary>
    public async Task ConnectAsync()
    {
        DiscordClient client = Build();
        await client.ConnectAsync();
    }
}
