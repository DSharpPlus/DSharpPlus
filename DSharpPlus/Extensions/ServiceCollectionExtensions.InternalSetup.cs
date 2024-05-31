using System.Net.Http;

using DSharpPlus.Net;
using DSharpPlus.Net.WebSocket;

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
            .AddSingleton<IClientErrorHandler, DefaultClientErrorHandler>();

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
        serviceCollection.Configure<DiscordConfiguration>(c => c.Intents = intents)
            .AddSingleton<IWebSocketClient, WebSocketClient>()
            .AddSingleton<PayloadDecompressor>()
            .AddSingleton<DiscordClient>();

        return serviceCollection;
    }
}
