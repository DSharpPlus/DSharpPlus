namespace DSharpPlus.Voice.MemoryServices.Channels;

/// <summary>
/// Represents a mechanism for writing to an <see cref="AudioChannel"/>. 
/// </summary>
public abstract class AudioChannelWriter
{
    /// <summary>
    /// Enqueues the specified leased buffer with the channel.
    /// </summary>
    public abstract bool TryWrite(AudioBufferLease buffer);

    /// <summary>
    /// Indicates to the channel that transmission is pausing for now.
    /// </summary>
    public abstract bool TryPause();

    /// <summary>
    /// Indicates to the channel that transmission is ending.
    /// </summary>
    public abstract void Terminate();
}
