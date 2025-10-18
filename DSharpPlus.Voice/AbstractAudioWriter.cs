using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides a basic mechanism for the different kinds of audio input DSharpPlus takes.
/// </summary>
public abstract class AbstractAudioWriter : PipeWriter
{
    private readonly VoiceConnection connection;

    /// <summary>
    /// The destination for encoded and prepared packets.
    /// </summary>
    protected ChannelWriter<AudioBufferLease> PacketWriter { get; }

    /// <summary>
    /// The amount of bytes consumed by a sample in the current audio format.
    /// </summary>
    protected internal abstract int SampleSize { get; }

    /// <summary>
    /// Indicates the parent connection to this writer.
    /// </summary>
    protected AbstractAudioWriter(VoiceConnection connection) 
        => this.connection = connection;

    /// <summary>
    /// Signals that no more audio is being written, but we intend to resume writing audio soon.
    /// </summary>
    public abstract void SignalSilence();

    /// <summary>
    /// Signals that this writer will not be used further. It is possible to re-request a writer of any given audio format.
    /// </summary>
    public virtual void SignalCompletion()
    {
        SignalSilence();
        this.connection
    }

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed override void Complete(Exception? exception = null)
    {
        if (exception is not null)
        {
            ExceptionDispatchInfo dispatch = ExceptionDispatchInfo.Capture(exception);
            dispatch.Throw();
        }

        SignalCompletion();
    }

    /// <inheritdoc/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override ValueTask CompleteAsync(Exception? exception = null)
    {
        Complete();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public sealed override Stream AsStream(bool leaveOpen = false) 
        => new AudioWriteStream(this, this.SampleSize);
}
