using System;
using System.ComponentModel;
using System.IO.Pipelines;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using DSharpPlus.Voice.MemoryServices.Channels;

namespace DSharpPlus.Voice;

/// <summary>
/// Provides a basic mechanism for the different kinds of audio input DSharpPlus takes.
/// </summary>
public abstract class AudioWriter : PipeWriter
{
    /// <summary>
    /// The destination for encoded and prepared packets.
    /// </summary>
    protected AudioChannelWriter PacketWriter { get; }

    /// <summary>
    /// Indicates the parent connection to this writer.
    /// </summary>
    protected AudioWriter(AudioChannelWriter writer) 
        => this.PacketWriter = writer;

    /// <summary>
    /// Signals that no more audio is being written, but we intend to resume writing audio soon.
    /// </summary>
    public virtual void SignalSilence()
        => this.PacketWriter.TryPause();

    /// <summary>
    /// Signals that this writer will not be used further. It is possible to re-request a writer of any given audio format.
    /// </summary>
    public virtual void SignalCompletion()
        => this.PacketWriter.Terminate();

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
}
