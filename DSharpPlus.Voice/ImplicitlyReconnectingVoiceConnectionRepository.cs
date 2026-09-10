using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides an implementation of <see cref="IVoiceConnectionRepository"/> that allows implicitly connecting to a new channel instead of throwing
/// and requiring the user to manually disconnect from the old channel. This mirrors the behaviour of the Discord client when clicking on a voice
/// channel while already being connected to a different voice channel 
/// </summary>
public sealed class ImplicitlyReconnectingVoiceConnectionRepository : IVoiceConnectionRepository
{
    private readonly ConcurrentDictionary<ulong, VoiceConnection> connections = [];

    /// <inheritdoc/>
    public IReadOnlyDictionary<ulong, VoiceConnection> Connections => this.connections;


    /// <inheritdoc/>
    public async Task RegisterConnectionAsync(ulong guildId, VoiceConnection connection)
    {
        if (this.connections.TryGetValue(guildId, out VoiceConnection? preexisting))
        {
            await preexisting.DisconnectAsync();
        }

        this.connections.AddOrUpdate(guildId, connection, (_, _) => connection);
    }

    /// <inheritdoc/>
    public void UnregisterConnection(ulong guildId)
        => _ = this.connections.Remove(guildId, out _);
}
