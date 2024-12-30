using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Channels;

using DSharpPlus.Clients;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Gateway;
using DSharpPlus.Net.InboundWebhooks;
using DSharpPlus.Net.InboundWebhooks.Transport;
using DSharpPlus.Net.Gateway.Compression;
using DSharpPlus.Net.Gateway.Compression.Zlib;
using DSharpPlus.Net.Gateway.Compression.Zstd;

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
            .AddKeyedSingleton("DSharpPlus.Gateway.EventChannel", Channel.CreateUnbounded<GatewayPayload>
            (
                new UnboundedChannelOptions 
                {
                    SingleReader = true 
                }
            ))
            .AddTransient<ITransportService, TransportService>()
            .AddTransient<IGatewayClient, GatewayClient>()
            .RegisterBestDecompressor()
            .AddSingleton<IShardOrchestrator, SingleShardOrchestrator>()
            .AddSingleton<IEventDispatcher, DefaultEventDispatcher>()
            .AddSingleton<DiscordClient>();

        // http events/interactions, if we're using those - doesn't actually cause any overhead if we aren't
        serviceCollection.AddKeyedSingleton("DSharpPlus.Webhooks.EventChannel", Channel.CreateUnbounded<DiscordWebhookEvent>
            (
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            ))
            .AddKeyedSingleton("DSharpPlus.Interactions.EventChannel", Channel.CreateUnbounded<DiscordHttpInteractionPayload>
            (
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            ))
            .AddSingleton<IInteractionTransportService, InteractionTransportService>()
            .AddSingleton<IWebhookTransportService, WebhookEventTransportService>();

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
            .AddKeyedSingleton("DSharpPlus.Gateway.EventChannel", Channel.CreateUnbounded<GatewayPayload>(new UnboundedChannelOptions { SingleReader = true }))
            .AddTransient<ITransportService, TransportService>()
            .AddTransient<IGatewayClient, GatewayClient>()
            .RegisterBestDecompressor()
            .AddSingleton<IShardOrchestrator, MultiShardOrchestrator>()
            .AddSingleton<IEventDispatcher, DefaultEventDispatcher>()
            .AddSingleton<DiscordClient>();

        // http events/interactions, if we're using those - doesn't actually cause any overhead if we aren't
        serviceCollection.AddKeyedSingleton("DSharpPlus.Webhooks.EventChannel", Channel.CreateUnbounded<DiscordWebhookEvent>
            (
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            ))
            .AddKeyedSingleton("DSharpPlus.Interactions.EventChannel", Channel.CreateUnbounded<DiscordHttpInteractionPayload>
            (
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            ))
            .AddSingleton<IInteractionTransportService, InteractionTransportService>()
            .AddSingleton<IWebhookTransportService, WebhookEventTransportService>();

        return serviceCollection;
    }

    private static IServiceCollection RegisterBestDecompressor(this IServiceCollection services)
    {
        if (NativeLibrary.TryLoad("libzstd", Assembly.GetEntryAssembly(), default, out _))
        {
            services.AddTransient<IPayloadDecompressor, ZstdDecompressor>();
        }
        else
        {
            services.AddTransient<IPayloadDecompressor, ZlibStreamDecompressor>();
        }

        return services;
    }
}
