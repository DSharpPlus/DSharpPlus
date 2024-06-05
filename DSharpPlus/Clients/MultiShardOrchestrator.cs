using System;
using System.Threading.Tasks;

using DSharpPlus.Net;
using DSharpPlus.Net.Gateway;
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

    public MultiShardOrchestrator
    (
        IServiceProvider serviceProvider, 
        IOptions<ShardingOptions> options,
        DiscordApiClient apiClient
    )
    {
        this.apiClient = apiClient;
        this.options = options.Value;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async ValueTask StartAsync()
    {
        uint startShards, totalShards, stride;

        if (this.options.ShardCount is null)
        {
            GatewayInfo info = await this.apiClient.GetGatewayInfoAsync();

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

        this.shards = new IGatewayClient[startShards];
    }
}
