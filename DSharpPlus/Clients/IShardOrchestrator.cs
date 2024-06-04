using System.Threading.Tasks;

namespace DSharpPlus.Clients;

/// <summary>
/// Represents a mechanism for orchestrating one or more shards in one or more processes.
/// </summary>
public interface IShardOrchestrator
{
    /// <summary>
    /// Starts all shards associated with this orchestrator.
    /// </summary>
    public ValueTask StartAsync();
}
