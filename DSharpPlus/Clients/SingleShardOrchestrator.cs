using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Net.Gateway;
using DSharpPlus.Net.WebSocket;

namespace DSharpPlus.Clients;

/// <summary>
/// Orchestrates a single "shard".
/// </summary>
public sealed class SingleShardOrchestrator : IShardOrchestrator
{
    private readonly IGatewayClient gatewayClient;
    private readonly DiscordApiClient apiClient;
    private readonly PayloadDecompressor decompressor;

    /// <summary>
    /// Creates a new instance of this type.
    /// </summary>
    public SingleShardOrchestrator
    (
        IGatewayClient gatewayClient,
        DiscordApiClient apiClient,
        PayloadDecompressor decompressor
    )
    {
        this.gatewayClient = gatewayClient;
        this.apiClient = apiClient;
        this.decompressor = decompressor;
    }

    /// <inheritdoc/>
    public bool AllShardsConnected => this.gatewayClient.IsConnected;

    /// <inheritdoc/>
    public TimeSpan GetConnectionLatency(ulong guildId) => this.gatewayClient.Ping;

    /// <inheritdoc/>
    public bool IsConnected(ulong guildId) => this.gatewayClient.IsConnected;

    /// <inheritdoc/>
    public async ValueTask ReconnectAsync() => await this.gatewayClient.ReconnectAsync();

    // guild ID doesn't matter here, since we only have a single shard
    /// <summary>
    /// Sends an outbound event to Discord.
    /// </summary>
    public async ValueTask SendOutboundEventAsync(byte[] payload, ulong _) 
        => await this.gatewayClient.WriteAsync(payload);

    /// <inheritdoc/>
    public async ValueTask StartAsync(DiscordActivity? activity, DiscordUserStatus? status, DateTimeOffset? idleSince)
    {
        GatewayInfo info = await this.apiClient.GetGatewayInfoAsync();

        QueryUriBuilder gwuri = new(info.Url);

        gwuri.AddParameter("v", "10")
             .AddParameter("encoding", "json");

        if (this.decompressor.CompressionLevel == GatewayCompressionLevel.Stream)
        {
            gwuri.AddParameter("compress", "zlib-stream");
        }

        await this.gatewayClient.ConnectAsync(gwuri.Build(), activity, status, idleSince);
    }

    /// <inheritdoc/>
    public async ValueTask StopAsync() => await this.gatewayClient.DisconnectAsync();
}
