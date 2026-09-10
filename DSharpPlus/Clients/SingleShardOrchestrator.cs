using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Net.Gateway;
using DSharpPlus.Net.Gateway.Compression;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Clients;

/// <summary>
/// Orchestrates a single "shard".
/// </summary>
public sealed class SingleShardOrchestrator : IShardOrchestrator
{
    private readonly IGatewayClient gatewayClient;
    private readonly DiscordRestApiClient apiClient;
    private readonly IPayloadDecompressor decompressor;
    private readonly ILogger<IShardOrchestrator> logger;

    /// <summary>
    /// Creates a new instance of this type.
    /// </summary>
    public SingleShardOrchestrator
    (
        IGatewayClient gatewayClient,
        DiscordRestApiClientFactory apiClientFactory,
        IPayloadDecompressor decompressor,
        ILogger<IShardOrchestrator> logger
    )
    {
        this.gatewayClient = gatewayClient;
        this.apiClient = apiClientFactory.GetCurrentApplicationClient();
        this.decompressor = decompressor;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public bool AllShardsConnected => this.gatewayClient.IsConnected;

    /// <inheritdoc/>
    public int TotalShardCount => 1;

    /// <inheritdoc/>
    public int ConnectedShardCount => 1;

    /// <inheritdoc/>
    public async ValueTask BroadcastOutboundEventAsync(byte[] payload)
    {
        if (!this.AllShardsConnected)
        {
            throw new InvalidOperationException("Broadcast is only possible when the shard is connected");
        }

        await SendOutboundEventAsync(payload, 0);
    }

    /// <inheritdoc/>
    public TimeSpan GetConnectionLatency(ulong _) => this.gatewayClient.Ping;

    /// <inheritdoc/>
    public TimeSpan GetConnectionLatency(int _) => this.gatewayClient.Ping;

    /// <inheritdoc/>
    public IEnumerable<int> GetShardIds() => [this.gatewayClient.ShardId];

    /// <inheritdoc/>
    public bool IsConnected(ulong _) => this.gatewayClient.IsConnected;

    /// <inheritdoc/>
    public bool IsConnected(int _) => this.gatewayClient.IsConnected;

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

        if (this.decompressor.IsTransportCompression)
        {
            gwuri.AddParameter("compress", this.decompressor.Name);
        }

        _ = RunGatewayAsync(gwuri.Build(), activity, status, idleSince);
    }

    private async Task RunGatewayAsync(string uri, DiscordActivity? activity, DiscordUserStatus? status, DateTimeOffset? idleSince)
    {
        Task<GatewayConnectionFrame> gatewayTask = this.gatewayClient.ConnectAsync(uri, activity, status, idleSince);

        while (true)
        {
            GatewayConnectionFrame frame = await gatewayTask;

            if (frame.DisconnectReason is GatewayDisconnectReason.UserRequested or GatewayDisconnectReason.IrrecoverableCloseCode)
            {
                this.logger.LogError
                (
                    frame.Exception,
                    "The gateway exited with disconnect reason {reason} and close code {closeCode}, abandoning reconnecting.", 
                    frame.DisconnectReason,
                    frame.CloseCode ?? (GatewayCloseCode)1000
                );

                break;
            }

            gatewayTask = this.gatewayClient.ReconnectAsync();
        }
    }

    /// <inheritdoc/>
    public async ValueTask StopAsync() => await this.gatewayClient.DisconnectAsync();
}
