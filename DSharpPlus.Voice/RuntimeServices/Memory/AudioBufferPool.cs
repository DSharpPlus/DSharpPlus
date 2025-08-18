using System;

namespace DSharpPlus.Voice.RuntimeServices.Memory;

/// <summary>
/// Provides a mechanism to enable reusing fixed-size audio buffers, represented as byte arrays.
/// </summary>
public abstract class AudioBufferPool
{
    // these are provided here as statics to help the JIT with devirtualizing them
    private static readonly SharedAudioBufferPool backing20 = new(972, TimeSpan.FromMilliseconds(20));
    private static readonly SharedAudioBufferPool backing120 = new(5772, TimeSpan.FromMilliseconds(120));
    private static readonly NonPoolingBufferPool backingSilence = new(15);

    /// <summary>
    /// A pre-provided pool for 20ms opus buffers.
    /// </summary>
    public static AudioBufferPool Opus20ms => backing20;

    /// <summary>
    /// A pre-provided pool for 120ms opus buffers.
    /// </summary>
    public static AudioBufferPool Opus120ms => backing120;

    /// <summary>
    /// A pre-provided pool for opus silence frames.
    /// </summary>
    public static AudioBufferPool OpusSilenceFrames => backingSilence;

    /// <summary>
    /// Acquires a lease to an array. Dispose of the lease to automatically return it to a pool.
    /// </summary>
    // we want to avoid making any guarantees here as to which pool it'll be returned to and keep the door open for changing the semantics of that operation
    public abstract AudioBufferLease Rent();

    /// <summary>
    /// Returns a leased buffer to a pool.
    /// </summary>
    // accessible only in implementing array pools or inside this assembly (meaning that users can define their own pool, but not otherwise call this method)
    protected internal abstract void Return(byte[] buffer);
}
