// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DSharpPlus.Etf.Serialization;

/*******************************************************************************************************
* here, we deal with parsing the ETF representation of integers into something .NET can use.
* 
* in general, we want to offer methods to convert terms to all sane .NET representations, both in Try-
* and throwing variants; and for integers specifically we want to both offer truncating and checked
* methods for slimming an integer down.
* 
* We want to define our Try- methods first and then implement throwing variants in the following form:
* 
* public readonly Thing GetThing()
* {
*     if (this.TryGetThing(out Thing thing))
*     {
*         return thing;
*     }
*     
*     ThrowHelper.ThrowInvalidDecode(typeof(Thing));
*     return default!;
* }
* 
* the last return doesn't actually matter, it is never invoked, roslyn just doesn't see through
* ThrowHelper calls always throwing and thinks this code is reachable.
*******************************************************************************************************/

partial struct EtfReader
{
    /// <summary>
    /// Provides the implementation for reading either a <seealso cref="TermType.SmallBig"/> or a
    /// <seealso cref="TermType.LargeBig"/> into a BigInteger.
    /// </summary>
    private readonly BigInteger ReadBigIntegerCore()
    {
        BigInteger value = new(this.CurrentTermContents[1..]);

        return MemoryMarshal.GetReference(this.CurrentTermContents) == 0 ? value : -value;
    }

    /// <summary>
    /// Provides the implementation for reading a 64-bit unsigned integer from a currently provided
    /// BigInteger term, ignoring the sign byte.
    /// </summary>
    private readonly ulong ReadUInt64FromBigInteger()
    {
        return Unsafe.ReadUnaligned<ulong>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents),
                elementOffset: 1
            )
        );
    }

    /// <summary>
    /// Provides the implementation for reading a 64-bit signed integer from a currently provided
    /// BigInteger term.
    /// </summary>
    private readonly long ReadInt64FromBigInteger()
    {
        long value = Unsafe.ReadUnaligned<long>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents), 
                elementOffset: 1
            )
        );

        return MemoryMarshal.GetReference(this.CurrentTermContents) == 0 ? value : -value;
    }

    /// <summary>
    /// Provides the implementation for reading a 32-bit unsigned integer from a currently provided
    /// BigInteger term, ignoring the sign byte.
    /// </summary>
    private readonly uint ReadUInt32FromBigInteger()
    {
        return Unsafe.ReadUnaligned<uint>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents),
                elementOffset: 1
            )
        );
    }

    /// <summary>
    /// Provides the implementation for reading a 32-bit signed integer from a currently provided
    /// BigInteger term.
    /// </summary>
    private readonly int ReadInt32FromBigInteger()
    {
        int value = Unsafe.ReadUnaligned<int>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents),
                elementOffset: 1
            )
        );

        return MemoryMarshal.GetReference(this.CurrentTermContents) == 0 ? value : -value;
    }

    /// <summary>
    /// Provides the implementation for reading a 16-bit unsigned integer from a currently provided
    /// BigInteger term, ignoring the sign byte.
    /// </summary>
    private readonly ushort ReadUInt16FromBigInteger()
    {
        return Unsafe.ReadUnaligned<ushort>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents),
                elementOffset: 1
            )
        );
    }

    /// <summary>
    /// Provides the implementation for reading a 16-bit signed integer from a currently provided
    /// BigInteger term.
    /// </summary>
    private readonly short ReadInt16FromBigInteger()
    {
        short value = Unsafe.ReadUnaligned<short>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents),
                elementOffset: 1
            )
        );

        return MemoryMarshal.GetReference(this.CurrentTermContents) == 0 ? value : (short)-value;
    }

    /// <summary>
    /// Provides the implementation for reading an 8-bit unsigned integer from a currently provided
    /// BigInteger term, ignoring the sign byte.
    /// </summary>
    private readonly byte ReadByteFromBigInteger()
    {
        return Unsafe.Add
        (
            ref MemoryMarshal.GetReference(this.CurrentTermContents),
            elementOffset: 1
        );
    }

    /// <summary>
    /// Provides the implementation for reading an 8-bit signed integer from a currently provided
    /// BigInteger term.
    /// </summary>
    private readonly sbyte ReadSByteFromBigInteger()
    {
        sbyte value = (sbyte)Unsafe.Add
        (
            ref MemoryMarshal.GetReference(this.CurrentTermContents),
            elementOffset: 1
        );

        return MemoryMarshal.GetReference(this.CurrentTermContents) == 0 ? value : (sbyte)-value;
    }
}
