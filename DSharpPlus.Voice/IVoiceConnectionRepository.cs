using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Voice;

/// <summary>
/// Represents a mechanism to manage and dispatch information to voice connections.
/// </summary>
public interface IVoiceConnectionRepository
{
    /// <summary>
    /// Provides voice connections indexed by guild ID.
    /// </summary>
    public IReadOnlyDictionary<ulong, VoiceConnection> Connections { get; }

    /// <summary>
    /// Registers a new voice connection.
    /// </summary>
    /// <remarks>
    /// Asynchronous implementations of this method must not throw exceptions.
    /// </remarks>
    /// <param name="guildId">The guild ID this connection takes place in.</param>
    /// <param name="connection">The newly created connection.</param>
    public Task RegisterConnectionAsync(ulong guildId, VoiceConnection connection);

    /// <summary>
    /// Unregisters a connection.
    /// </summary>
    /// <param name="guildId">The guild ID this connection took place in.</param>
    public void UnregisterConnection(ulong guildId);
}
