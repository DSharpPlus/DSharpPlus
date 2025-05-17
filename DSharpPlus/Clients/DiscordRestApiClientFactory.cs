using System;
using System.Threading.Tasks;

using DSharpPlus.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Clients;

/// <summary>
/// Provides a way to get access to either the current application's REST client or OAuth2-based REST clients.
/// </summary>
public sealed class DiscordRestApiClientFactory
{
    private readonly DiscordRestApiClient primaryClient;
    private readonly IServiceProvider services;
    private readonly IMemoryCache clientCache;

    private readonly MemoryCacheEntryOptions options;

    public DiscordRestApiClientFactory
    (
        DiscordRestApiClient primaryClient,
        IOptions<TokenContainer> tokenContainer,
        IMemoryCache cache,
        IServiceProvider services
    )
    {
        this.services = services;
        this.primaryClient = primaryClient;
        this.clientCache = cache;

        this.primaryClient.SetToken(TokenType.Bot, tokenContainer.Value.GetToken());

        this.options = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };
    }

    /// <summary>
    /// Gets the REST API client for the current bot.
    /// </summary>
    public DiscordRestApiClient GetCurrentApplicationClient()
        => this.primaryClient;

    /// <summary>
    /// Gets a REST API client for a given bearer token. Not all features of the API might be available.
    /// </summary>
    public async Task<DiscordRestApiClient> GetOAuth2ClientAsync(string token)
    {
        if (this.clientCache.TryGetValue($"dsharpplus.oauth-2-client:bearer-{token}", out OAuth2DiscordClient? cachedClient))
        {
            return cachedClient.ApiClient;
        }

        OAuth2DiscordClient client = new();
        DiscordRestApiClient apiClient = this.services.GetRequiredService<DiscordRestApiClient>();

        apiClient.SetToken(TokenType.Bearer, token);

        apiClient.SetClient(client);
        client.ApiClient = apiClient;

        this.clientCache.Set($"dsharpplus.oauth-2-client:bearer-{token}", client, this.options);

        await client.InitializeAsync();

        return client.ApiClient;
    }
}
