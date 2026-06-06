using System.Collections.Generic;

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
    /// <param name="guildId">The guild ID this connection takes place in.</param>
    /// <param name="connection">The newly created connection.</param>
    public void RegisterConnection(ulong guildId, VoiceConnection connection);

    /// <summary>
    /// Unregisters a connection.
    /// </summary>
    /// <param name="guildId">The guild ID this connection took place in.</param>
    public void UnregisterConnection(ulong guildId);
}
