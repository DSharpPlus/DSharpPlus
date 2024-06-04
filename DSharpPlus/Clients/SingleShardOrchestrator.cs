using System.Threading.Tasks;

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
    public ValueTask StartAsync() => throw new System.NotImplementedException();
}
