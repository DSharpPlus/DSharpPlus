using System.Collections.Concurrent;
using System.Collections.Generic;

using DSharpPlus.Voice.Exceptions;

namespace DSharpPlus.Voice;

/// <inheritdoc cref="IVoiceConnectionRepository"/>
public sealed class VoiceConnectionRepository : IVoiceConnectionRepository
{
    private readonly ConcurrentDictionary<ulong, VoiceConnection> connections = [];

    /// <inheritdoc/>
    public IReadOnlyDictionary<ulong, VoiceConnection> Connections => this.connections;


    /// <inheritdoc/>
    public void RegisterConnection(ulong guildId, VoiceConnection connection)
    {
        if (!this.connections.TryAdd(guildId, connection))
        {
            throw new ConnectingFailedException("A connection to this guild already exists and was not previously cancelled.");
        }
    }

    /// <inheritdoc/>
    public void UnregisterConnection(ulong guildId)
        => _ = this.connections.Remove(guildId, out _);
}
