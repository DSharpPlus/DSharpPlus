using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Gateway;
using DSharpPlus.Net.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Clients;

/// <summary>
/// Orchestrates multiple shards within this process.
/// </summary>
public sealed class MultiShardOrchestrator : IShardOrchestrator
{
    private IGatewayClient[] shards;
    private readonly DiscordApiClient apiClient;
    private readonly ShardingOptions options;
    private readonly IServiceProvider serviceProvider;
    private readonly PayloadDecompressor decompressor;

    public MultiShardOrchestrator
    (
        IServiceProvider serviceProvider, 
        IOptions<ShardingOptions> options,
        DiscordApiClient apiClient,
        PayloadDecompressor decompressor
    )
    {
        this.apiClient = apiClient;
        this.options = options.Value;
        this.serviceProvider = serviceProvider;
        this.decompressor = decompressor;
    }

    /// <inheritdoc/>
    public async ValueTask StartAsync(DiscordActivity? activity, DiscordUserStatus? status, DateTimeOffset? idleSince)
    {
        uint startShards, totalShards, stride;
        GatewayInfo info = await this.apiClient.GetGatewayInfoAsync();

        if (this.options.ShardCount is null)
        {
            startShards = totalShards = (uint)info.ShardCount;
            stride = 0;
        }
        else
        {
            totalShards = this.options.TotalShards == 0 ? this.options.ShardCount.Value : this.options.TotalShards;
            startShards = this.options.ShardCount.Value;
            stride = this.options.Stride;

            if (stride != 0 && totalShards == 0)
            {
                throw new ArgumentOutOfRangeException
                (
                    paramName: "options",
                    message: "The sharded client was set up for multi-process sharding but did not specify a total shard count."
                );
            }
        }

        QueryUriBuilder gwuri = new(info.Url);

        gwuri.AddParameter("v", "10")
             .AddParameter("encoding", "json");

        if (this.decompressor.CompressionLevel == GatewayCompressionLevel.Stream)
        {
            gwuri.AddParameter("compress", "zlib-stream");
        }

        this.shards = new IGatewayClient[startShards];

        for (int i = 0; i < startShards; i++)
        {
            this.shards[i] = this.serviceProvider.GetRequiredService<IGatewayClient>();

            await this.shards[i].ConnectAsync
            (
                gwuri.Build(),
                activity,
                status,
                idleSince,
                new ShardInfo
                {
                    ShardCount = (int)totalShards,
                    ShardId = (int)stride + i
                }
            );
        }
    }

    public async ValueTask StopAsync()
    {
        foreach (IGatewayClient client in this.shards)
        {
            await client.DisconnectAsync();
        }
    }
}
