using System.Net.Http;
using System.Threading.Channels;

using DSharpPlus.Clients;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Gateway;
using DSharpPlus.Net.Gateway.Compression;
using DSharpPlus.Net.Gateway.Compression.Zlib;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Extensions;

public static partial class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDSharpPlusDefaultsSingleShard
    (
        this IServiceCollection serviceCollection,
        DiscordIntents intents
    )
    {
        // peripheral setup
        serviceCollection.AddMemoryCache()
            .AddSingleton<IMessageCacheProvider, MessageCache>()
            .AddSingleton<IClientErrorHandler, DefaultClientErrorHandler>()
            .AddSingleton<IGatewayController, DefaultGatewayController>();

        // rest setup
        serviceCollection.AddKeyedSingleton<HttpClient>("DSharpPlus.Rest.HttpClient")
            .AddSingleton<DiscordApiClient>()
            .AddSingleton<RestClient>
            (
                serviceProvider =>
                {
                    HttpClient client = serviceProvider.GetRequiredKeyedService<HttpClient>("DSharpPlus.Rest.HttpClient");
                    ILogger<RestClient> logger = serviceProvider.GetRequiredService<ILogger<RestClient>>();
                    IOptions<RestClientOptions> options = serviceProvider.GetRequiredService<IOptions<RestClientOptions>>();
                    IOptions<TokenContainer> token = serviceProvider.GetRequiredService<IOptions<TokenContainer>>();

                    return new(logger, client, options, token);
                }
            );

        // gateway setup
        serviceCollection.Configure<GatewayClientOptions>(c => c.Intents = intents)
            .AddKeyedSingleton("DSharpPlus.Gateway.EventChannel", Channel.CreateUnbounded<GatewayPayload>())
            .AddTransient<ITransportService, TransportService>()
            .AddTransient<IGatewayClient, GatewayClient>()
            .AddTransient<IPayloadDecompressor, ZlibStreamDecompressor>()
            .AddSingleton<IShardOrchestrator, SingleShardOrchestrator>()
            .AddSingleton<DiscordClient>();

        return serviceCollection;
    }

    internal static IServiceCollection AddDSharpPlusDefaultsSharded
    (
        this IServiceCollection serviceCollection,
        DiscordIntents intents
    )
    {
        // peripheral setup
        serviceCollection.AddMemoryCache()
            .AddSingleton<IMessageCacheProvider, MessageCache>()
            .AddSingleton<IClientErrorHandler, DefaultClientErrorHandler>()
            .AddSingleton<IGatewayController, DefaultGatewayController>();

        // rest setup
        serviceCollection.AddKeyedSingleton<HttpClient>("DSharpPlus.Rest.HttpClient")
            .AddSingleton<DiscordApiClient>()
            .AddSingleton<RestClient>
            (
                serviceProvider =>
                {
                    HttpClient client = serviceProvider.GetRequiredKeyedService<HttpClient>("DSharpPlus.Rest.HttpClient");
                    ILogger<RestClient> logger = serviceProvider.GetRequiredService<ILogger<RestClient>>();
                    IOptions<RestClientOptions> options = serviceProvider.GetRequiredService<IOptions<RestClientOptions>>();
                    IOptions<TokenContainer> token = serviceProvider.GetRequiredService<IOptions<TokenContainer>>();

                    return new(logger, client, options, token);
                }
            );

        // gateway setup
        serviceCollection.Configure<GatewayClientOptions>(c => c.Intents = intents)
            .AddKeyedSingleton("DSharpPlus.Gateway.EventChannel", Channel.CreateUnbounded<GatewayPayload>())
            .AddTransient<ITransportService, TransportService>()
            .AddTransient<IGatewayClient, GatewayClient>()
            .AddTransient<IPayloadDecompressor, ZlibStreamDecompressor>()
            .AddSingleton<IShardOrchestrator, MultiShardOrchestrator>()
            .AddSingleton<DiscordClient>();

        return serviceCollection;
    }
}
