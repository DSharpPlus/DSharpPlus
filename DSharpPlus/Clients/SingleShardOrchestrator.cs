using System.Threading.Tasks;

using DSharpPlus.Net.Gateway;

namespace DSharpPlus.Clients;

/// <summary>
/// Orchestrates a single "shard".
/// </summary>
public sealed class SingleShardOrchestrator : IShardOrchestrator
{
    private readonly IGatewayClient gatewayClient;

    /// <summary>
    /// Creates a new instance of this type.
    /// </summary>
    public SingleShardOrchestrator(IGatewayClient gatewayClient)
        => this.gatewayClient = gatewayClient;

    /// <inheritdoc/>
    public async ValueTask StartAsync() => await this.gatewayClient.ConnectAsync();

    /// <inheritdoc/>
    public async ValueTask StopAsync() => await this.gatewayClient.DisconnectAsync();
}
