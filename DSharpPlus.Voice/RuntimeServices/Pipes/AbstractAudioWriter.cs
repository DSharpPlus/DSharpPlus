using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

/// <summary>
/// Provides a basic mechanism for the different kinds of audio input DSharpPlus takes.
/// </summary>
public abstract class AbstractAudioWriter : PipeWriter
{
    /// <summary>
    /// Gets the current writing position.
    /// </summary>
    public abstract long Position { get; }

    /// <summary>
    /// The amount of bytes consumed by a sample in the current audio format.
    /// </summary>
    protected internal abstract int SampleSize { get; }

    /// <summary>
    /// Signals that no more audio is being written, but we intend to resume writing audio soon.
    /// </summary>
    public abstract void SignalSilence();

    /// <summary>
    /// Signals that this writer will not be used further. It is possible to re-request a writer of any given audio format.
    /// </summary>
    public abstract void SignalCompletion();

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
