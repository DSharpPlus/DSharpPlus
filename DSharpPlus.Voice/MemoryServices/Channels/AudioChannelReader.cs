using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.MemoryServices.Channels;

/// <summary>
/// Represents a mechanism for reading from an <see cref="AudioChannel"/>. 
/// </summary>
public abstract class AudioChannelReader
{
    /// <summary>
    /// Attempts to read a single item synchronously from the channel.
    /// </summary>
    public abstract AudioChannelReadResult TryRead();

    /// <summary>
    /// Attempts to read an item from the channel once it becomes available.
    /// </summary>
    public abstract Task<AudioChannelReadResult> ReadAsync(CancellationToken ct);

    /// <summary>
    /// Waits until either an item is available for reading or the channel is terminated.
    /// </summary>
    public abstract Task WaitToReadAsync(CancellationToken ct);
}
