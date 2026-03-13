using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.MemoryServices.Channels;

/// <summary>
/// Represents a way to communicate audio data and signals between audio writers and the connection.
/// </summary>
public sealed partial class AudioChannel
{
    private Channel<AudioBufferLease> underlying;
    private readonly DefaultReader reader;
    private readonly DefaultWriter writer;
    private readonly TaskCompletionSource terminationTcs;
    private bool isPaused;

    public AudioChannel()
    {
        this.underlying = Channel.CreateUnbounded<AudioBufferLease>();
        this.reader = new(this);
        this.writer = new(this);
        this.terminationTcs = new();
    }

    /// <summary>
    /// Indicates whether this channel has been requested for termination. This will end all future transmission.
    /// </summary>
    public bool IsTerminationRequested { get; private set; }

    /// <summary>
    /// Serves as a mechanism to read from this channel.
    /// </summary>
    public AudioChannelReader Reader => this.reader;

    /// <summary>
    /// Serves as a mechanism to write to this channel.
    /// </summary>
    public AudioChannelWriter Writer => this.writer;

    /// <summary>
    /// Clears all elements currently enqueued into the audio channel. This method is thread-safe.
    /// </summary>
    public void Clear()
    {
        Channel<AudioBufferLease> old = Interlocked.Exchange(ref this.underlying, Channel.CreateUnbounded<AudioBufferLease>());

        while (old.Reader.TryRead(out AudioBufferLease lease))
        {
            lease.Dispose();
        }
    }
}
