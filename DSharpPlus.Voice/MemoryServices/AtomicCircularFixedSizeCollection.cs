using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// A fixed-size, lock-free and thread-safe circular collection of a power of two of byte arrays. All byte arrays in the 
/// collection are of the same size and are directly owned by the collection. When rented, arrays are released from the 
/// ownership of the collection and don't necessarily need to be returned back to this collection: the pool may decide to 
/// return them to a different collection.
/// </summary>
/// <remarks>
/// In principle, this type functions as a ring buffer, but there are several cuts made: <br/>
/// <br/>
/// 1. Since we're doing this lock-free, a different thread may take the logically "next" element in the collection while 
/// we're preparing; so instead of taking an element and returning it we have to loop until we find an element not yet being 
/// contested by another thread. To prevent an infinite loop, we only loop at most one full "round" over the collection 
/// before deciding that this collection isn't worth our time.
/// <br/>
/// 2. Likewise, a different thread may return to the logically "next" element in the collection, so we have to do the same 
/// looping process when returning an element.
/// <br/>
/// 3. Since other threads may use the stored read/write indices at the same time as we are, we must take care to update them 
/// atomically. The size of this collection is defined as a power of two, so we don't need to store them clamped to fit within 
/// the range and can just let them overflow: access to the backing storage can just mask out higher bits.
/// <br/>
/// 4. Unlike a traditional collection, this type allows fragmentation of the underlying storage. We expect to see enough 
/// throughput to amortize any fragmentation costs in scenarios where performance of this type matters, and the looping to 
/// next elements discussed in points 1 and 2 will solve the problem of fragmentation leading to misses for us.
/// </remarks>
//
// an argument could be made that this isn't a ring buffer or circular buffer at all, and indeed the circling is more of a
// convenient way to get an exit condition and doesn't really serve much of a storage layout purpose. the exit condition is
// important because this is ultimately just a segment of the final pool - we have one segment per core, and then a pool of
// global segments that get tracked and trimmed regularly. we need a way to get out of this pool once we think it's no
// longer profitable to stay (since fully looping around implies the collection is full/empty, depending on operation), and
// this is a convenient way to do so. the above explanation mostly serves to illuminate the concept, not so much as a
// compsci-theory accurate description of the concept.
// on an aside, it's okay if the read/write indices overflow back to zero - we only care about the final 7 bits. only
// reason they're not just byte values is that Interlocked.Increment is, on .NET 9, only available on 32-bit and 64-bit
// integers; and it also doesn't change the overall size of the object: any saved bytes would just become padding instead.
// theoretically, if a future .NET version adds smaller overloads to Increment, we could use smaller integer types, but
// it would only matter on 32-bit, which we don't support anyway.
// furthermore, while there's in theory nothing that speaks against making this a struct, keeping this as a class just
// makes it easier to ensure we're looking at the same read and write indices from all threads, particularly if this is a
// global pool segment rather than a per-core dedicated segment (though, this is technically still observable with
// per-core segments if the scheduler decides to mess with us). since we'd be passing those by ref all the time anyways,
// this might as well be a class. 
internal sealed class AtomicCircularFixedSizeCollection
{
    private const uint AccessMask = 0b00000000_00000000_00000000_01111111;
    private readonly byte[]?[] buffers;
    private uint readIndex;
    private uint writeIndex;

    public AtomicCircularFixedSizeCollection(int arraySize, bool skipInitialization = false)
    {
        this.buffers = new byte[128][];

        for (int i = 0; i < 128; i++)
        {
            this.buffers[i] = skipInitialization ? null : new byte[arraySize];
        }

        this.readIndex = 0;

        // logically, this is at the end of the collection, so 128 - but that wraps around to 0, so we'll just use 0 here.
        this.writeIndex = 0;
    }

    public bool TryGetArray([MaybeNullWhen(false)] out byte[]? array)
    {
        uint startingIndex = this.readIndex & AccessMask;

        while (true)
        {
            array = Interlocked.Exchange(ref this.buffers[this.readIndex & AccessMask], null);

            Interlocked.Increment(ref this.readIndex);

            if (array is not null)
            {
                return true;
            }

            if ((this.readIndex & AccessMask) == startingIndex)
            {
                return false;
            }
        }
    }

    // when iterating here, the array ref can take on a different array returned by a different thread. it's fine if
    // it does that. we'll just transfer that array we've taken out to a new slot. this would be slightly problematic
    // for a public type, but ultimately this is an implementation type and we're allowed to pull shenanigans like that
    public bool TryReturnArray([MaybeNullWhen(true)] ref byte[]? array)
    {
        uint startingIndex = this.writeIndex & AccessMask;

        while (true)
        {
            if (this.buffers[this.writeIndex & AccessMask] is null)
            {
                array = Interlocked.Exchange(ref this.buffers[this.writeIndex & AccessMask], array);
            }

            Interlocked.Increment(ref this.writeIndex);

            if (array is null)
            {
                // we successfully returned
                return true;
            }

            // either the array wasn't null 

            if ((this.writeIndex & AccessMask) == startingIndex)
            {
                return false;
            }
        }
    }

    // only safe to call after this collection has been removed from rotation
    public int CountNonNullSlots() 
        => this.buffers.Count(x => x is not null);
}
