using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;

using Timer = System.Timers.Timer;

namespace DSharpPlus.Voice.RuntimeServices.Memory;

/// <summary>
/// An implementation of <see cref="AudioBufferPool"/> focusing on providing a large, fast pool to all connections
/// managed by the extension that move large amounts of memory at once.
/// </summary>
public sealed class SharedAudioBufferPool : AudioBufferPool
{
    private readonly record struct Stats(int PooledArrays, int TotalCapacity, int EstimatedConnections);

    private readonly int bufferSize;
    private readonly int processors;

    private readonly float bufferPerIntervalFactor;

    private readonly AtomicCircularFixedSizeCollection[] perCoreCollections;
    private readonly ConcurrentBag<AtomicCircularFixedSizeCollection> globalCollections;
    private int mask;

    private readonly RingBuffer<int> pastEstimatedConnections;
    private readonly Timer statCollectionTimer;

    private int pooledArrays;
    private int returnedArrays;

    public SharedAudioBufferPool(int bufferSize, TimeSpan durationPerBuffer)
    {
        this.bufferSize = bufferSize;
        this.processors = Environment.ProcessorCount;

        this.bufferPerIntervalFactor = (float)(durationPerBuffer.TotalMilliseconds / 3000);

        this.perCoreCollections = new AtomicCircularFixedSizeCollection[this.processors];
        this.globalCollections = [];
        this.mask = 0b0011;

        for (int i = 0; i < this.processors && i <= this.mask; i++)
        {
            this.perCoreCollections[i] = new(this.bufferSize);
        }

        this.pastEstimatedConnections = new(5);
        this.statCollectionTimer = new(TimeSpan.FromSeconds(3));
        this.statCollectionTimer.Elapsed += CollectAndProcessStats;
        this.statCollectionTimer.Start();
    }

    /// <inheritdoc/>
    public override AudioBufferLease Rent()
    {
        if (this.perCoreCollections[Thread.GetCurrentProcessorId() & (int)this.mask].TryGetArray(out byte[]? array))
        {
            Interlocked.Decrement(ref this.pooledArrays);
            return new(this, array);
        }

        foreach (AtomicCircularFixedSizeCollection collection in this.globalCollections)
        {
            if (collection.TryGetArray(out array))
            {
                Interlocked.Decrement(ref this.pooledArrays);
                return new(this, array);
            }
        }

        return new(this, new byte[this.bufferSize]);
    }

    /// <inheritdoc/>
    protected internal override void Return(byte[]? buffer)
    {
        Debug.Assert(buffer is not null && buffer.Length == this.bufferSize);

        Interlocked.Increment(ref this.returnedArrays);

        if (this.perCoreCollections[Thread.GetCurrentProcessorId() & (int)this.mask].TryReturnArray(ref buffer))
        {
            Interlocked.Increment(ref this.pooledArrays);
            return;
        }

        foreach (AtomicCircularFixedSizeCollection collection in this.globalCollections)
        {
            if (collection.TryReturnArray(ref buffer))
            {
                Interlocked.Increment(ref this.pooledArrays);
                return;
            }
        }
    }

    // here, we figure out the current state of the pool and decide whether we need to grow it.
    //
    // in the early stages, before we fill out the per-core collections, we grow it whenever we run low on arrays at all.
    // later on, we try to estimate the amount of active connections and make sure we have an appropriate amount of space as well
    // as available buffers. we use a three-second interval for checking stats
    private void CollectAndProcessStats(object? _, ElapsedEventArgs __)
    {
        Stats stats = new
        (
            PooledArrays: this.pooledArrays,
            TotalCapacity: (this.mask * 128) + (this.globalCollections.Count * 128),
            EstimatedConnections: (int)(this.returnedArrays * this.bufferPerIntervalFactor)
        );

        this.pooledArrays = 0;
        this.returnedArrays = 0;

        this.pastEstimatedConnections.Add(stats.EstimatedConnections);

        // we're not yet fully grown and we're at ten percent pool capacity or less. presume we're essentially out.
        if (this.mask <= 255)
        {
            if (stats.PooledArrays < stats.TotalCapacity * 0.10)
            {
                int newMask = (this.mask << 1) + 1;

                if (newMask == 255)
                {
                    newMask = int.MaxValue;
                }

                for (int i = this.mask + 1; i < this.processors && i <= newMask; i++)
                {
                    this.perCoreCollections[i] = new(this.bufferSize);
                }

                this.mask = newMask;
            }

            return;
        }

        // we're at less than 65% fill rate, let's just allocate through the next 3s interval - we probably don't have enough arrays in circulation.
        if (stats.PooledArrays < stats.TotalCapacity * 0.65)
        {
            return;
        }

        int connections = (int)this.pastEstimatedConnections.Average();

        // we assume a capacity of 128 arrays per active connection in the pool, but we don't want to spike all too much, so we'll add or remove at most two
        // arrays per iteration here
        int desiredCapacity = connections * 128;

        if (stats.TotalCapacity < desiredCapacity - 256)
        {
            this.globalCollections.Add(new(this.bufferSize, true));
            this.globalCollections.Add(new(this.bufferSize, true));
        }
        else if (stats.TotalCapacity > desiredCapacity + 256)
        {
            Console.WriteLine("Decreasing capacity");

            this.globalCollections.TryTake(out AtomicCircularFixedSizeCollection? collection);

            if (collection is not null)
            {
                Interlocked.Add(ref this.pooledArrays, -collection.CountNonNullSlots());
            }

            this.globalCollections.TryTake(out collection);

            if (collection is not null)
            {
                Interlocked.Add(ref this.pooledArrays, -collection.CountNonNullSlots());
            }
        }
    }
}
