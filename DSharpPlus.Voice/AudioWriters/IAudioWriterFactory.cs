using System.Threading.Channels;

using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.AudioWriters;

/// <summary>
/// Represents a mechanism to create audio writers of a particular format.
/// </summary>
public interface IAudioWriterFactory
{
    /// <summary>
    /// Creates an audio writer of the desired format.
    /// </summary>
    /// <param name="format">The format to create an audio writer for.</param>
    /// <param name="connection">The main connection object.</param>
    /// <param name="writer">The sink for encoded packets.</param>
    /// <returns>The newly created audio writer.</returns>
    public AbstractAudioWriter CreateAudioWriter(AudioFormat format, VoiceConnection connection, ChannelWriter<AudioBufferLease> writer);
}
