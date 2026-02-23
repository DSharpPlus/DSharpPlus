using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DSharpPlus.Voice.MemoryServices;

internal static partial class MemoryHelpers
{
    // we add 1 so as to never round down and not have enough space. sometimes we might not need that last element, but it's fine.
    public static int CalculateNeededSamplesFor48KHz(int inputSampleRate, int inputSampleCount) 
        => (int)((inputSampleRate / 48000.0 * inputSampleCount) + 1);

    /// <summary>
    /// Reinterprets an instance of <see cref="Memory{T}"/> to one holding a different type.
    /// </summary>
    /// <remarks>
    /// This is only safe for unmanaged TFrom and TTo whose sizes are clean multiples, with TFrom being smaller or equal.
    /// </remarks>
    /// <param name="memory">The <see cref="Memory{T}"/> instance to reinterpret.</param>
    /// <typeparam name="TFrom">The original data type of the memory region.</typeparam>
    /// <typeparam name="TTo">The desired data type of the memory region.</typeparam>
    /// <returns>A reinterpreted <see cref="Memory{T}"/> instance pointing to the same memory region.</returns>
    public static unsafe Memory<TTo> CastMemory<TFrom, TTo>(Memory<TFrom> memory)
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        // verify that TTo is larger or equal to TFrom and that TTo is a clean multiple (this isn't technically required by the implementation,
        // but otherwise this is significantly more awful and significantly less safe, so)
        Debug.Assert(sizeof(TFrom) <= sizeof(TTo));
        Debug.Assert(sizeof(TTo) % sizeof(TFrom) == 0);

        int factor = sizeof(TTo) / sizeof(TFrom);

        Memory<TTo> target = Unsafe.As<Memory<TFrom>, Memory<TTo>>(ref memory);
        
        // since we're working with a clean multiple, we know that there are length / factor elements in the provided memory. however, since we just
        // reinterpret-casted, the length of the new instance is still the original, which is only safe for same-size casts. therefore, "slice" it
        // down to the amount of elements we know exist - this isn't actually a real slice, we just want to set the length field to the correct value.
        return target[..(memory.Length / factor)];
    }
}
