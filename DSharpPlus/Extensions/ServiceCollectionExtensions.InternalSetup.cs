using System.Net.Http;
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
using System;
using System.Reflection;
using System.IO;

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
        if (NativeLibrary.TryLoad("libzstd", out _) || DeriveCurrentRidAndTestZstd())
        {
            services.AddTransient<IPayloadDecompressor, ZstdDecompressor>();
        }
        else
        {
            services.AddTransient<IPayloadDecompressor, ZlibStreamDecompressor>();
        }

        return services;

        static bool DeriveCurrentRidAndTestZstd()
        {
            string dsharpplusPath = Assembly.GetCallingAssembly().Location.TrimEnd("DSharpPlus.dll");

            if (OperatingSystem.IsWindows())
            {
                // zstd is supported on win-x64 and win-arm64
                string? path = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => Path.Join(dsharpplusPath, "runtimes/win-x64/native/libzstd.dll"),
                    Architecture.Arm64 => Path.Join(dsharpplusPath, "runtimes/win-arm64/native/libzstd.dll"),
                    _ => null
                };

                return path is not null && NativeLibrary.TryLoad(path, out _);
            }

            if (OperatingSystem.IsLinux())
            {
                // zstd is supported on linux-x64 and linux-arm64
                string? path = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => Path.Join(dsharpplusPath, "runtimes/linux-x64/native/libzstd.so"),
                    Architecture.Arm64 => Path.Join(dsharpplusPath, "runtimes/linux-arm64/native/libzstd.so"),
                    _ => null
                };

                return path is not null && NativeLibrary.TryLoad(path, out _);
            }

            if (OperatingSystem.IsMacOS())
            {
                // zstd is supported on osx, either x64 or arm64 - we have one "fat" dylib for both targets, so
                string? path = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 or Architecture.Arm64 => Path.Join(dsharpplusPath, "runtimes/osx/native/libzstd.dylib"),
                    _ => null
                };

                return path is not null && NativeLibrary.TryLoad(path, out _);
            }

            return false;
        }
    }
}
