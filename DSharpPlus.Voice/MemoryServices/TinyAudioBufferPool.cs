using System.Collections.Concurrent;
using System.Threading;

namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// An implementation of <see cref="AudioBufferPool"/> focusing on providing cyclic pooling for a single, streaming connection where
/// audio is provided in approximately real-time to the extension.
/// </summary>
public sealed class TinyAudioBufferPool : AudioBufferPool
{
    private byte[]? cache;
    private readonly ConcurrentQueue<byte[]> arrays;
    private int arraySize;

    /// <summary>
    /// Creates a new instance with the specified starting array size.
    /// </summary>
    public TinyAudioBufferPool(int arraySize)
    {
        this.arraySize = arraySize;
        this.arrays = [];

        this.cache = new byte[arraySize];
    }

    /// <summary>
    /// Rents an array from the pool and returns a lease to it. The array will be automatically returned by disposing the lease.
    /// </summary>
    public override AudioBufferLease Rent()
    {
        byte[]? array;

        if ((array = Interlocked.Exchange(ref this.cache, null)) is not null)
        {
            return new(this, array);
        }

        if (this.arrays.TryDequeue(out array))
        {
            return new(this, array);
        }
        else
        {
            return new(this, new byte[this.arraySize]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buffer"></param>
    protected internal override void Return(byte[] buffer)
    {
        if (this.arraySize != buffer.Length)
        {
            // drop it, this array was rented before we changed audio mode
            return;
        }

        if (this.cache is null)
        {
            if (Interlocked.Exchange(ref this.cache, buffer) == null)
            {
                // we successfully returned this buffer into the thread-local cache
                return;
            }
        }

        // don't allow this to become too big
        if (this.arrays.Count <= 32)
        {
            this.arrays.Enqueue(buffer);
        }
    }

    /// <summary>
    /// Whenever we change the audio mode, we need to change the size of the arrays used. It's okay if this misses an array here and there,
    /// but generally the arrays should be switched out as quickly as possible.
    /// </summary>
    public void ChangePooledArraySize(int newArraySize)
    {
        this.arraySize = newArraySize;
        this.cache = null;
        this.arrays.Clear();
    }
}
