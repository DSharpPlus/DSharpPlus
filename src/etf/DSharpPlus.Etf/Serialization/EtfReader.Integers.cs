// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using DSharpPlus.Etf.Extensions;

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
* public readonly Thing ReadThing()
* {
*     if (this.TryReadThing(out Thing thing))
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

    /***************************************************************************************************
    * in the following section, we deal with handling BigInteger terms into primitive .NET
    * representations. this is implemented using two very safe generic math methods that totally
    * abstain from all cursed-ness...
    * 
    * big integers are LITTLE ENDIAN, as opposed to everything else in the entire format, and we will
    * make use of that. this code is written under the (reasonable) assumption that big endian machines
    * are dead and that it will only run on little endian systems.
    * 
    * there are rationales for all decisions, however scuffed they might look:
    * 
    * 1. by using Unsafe.Add we elide a slice; and the following code is illegal under current ref
    *    safety rules, so we can't simplify it:
    *    TNumber value = Unsafe.ReadUnaligned<TNumber>(ref this.CurrentTermContents[1]);
    *    
    * 2. using Unsafe.ReadUnaligned over TNumber.ReadLittleEndian is done for the same reason:
    *    avoiding the slice associated with starting at byte 1 rather than byte 0
    *    
    * 3. in the signed implementation we use MemoryMarshal.GetReference(this.CurrentTermContents) over
    *    just this.CurrentTermContents[0], which elides a range check that at this point we know to be
    *    unnecessary but that still poses a branch the branch predictor has to deal with. oddly,
    *    removing that branch also causes the JIT to optimize the ternary operator to a conditional
    *    move, which actually makes this code entirely branchless.
    ***************************************************************************************************/

    /// <summary>
    /// Provides the implementation for reading a unsigned integer from a currently provided
    /// BigInteger term, ignoring the sign byte.
    /// </summary>
    private readonly unsafe TNumber ReadUnsignedIntegerFromBigInteger<TNumber>()
        where TNumber : unmanaged, IUnsignedNumber<TNumber>, IBinaryInteger<TNumber>
    {
        TNumber value = TNumber.Zero;

        Unsafe.CopyBlockUnaligned
        (
            destination: &value,
            source: Unsafe.AsPointer
            (
                ref Unsafe.Add
                (
                    source: ref MemoryMarshal.GetReference(this.CurrentTermContents),
                    elementOffset: 1
                )
            ),
            byteCount: (uint)this.CurrentTermContents.Length - 1
        );

        return value;
    }

    /// <summary>
    /// Provides the implementation for reading a signed integer from a currently provided
    /// BigInteger term.
    /// </summary>
    private readonly unsafe TNumber ReadSignedIntegerFromBigInteger<TNumber>()
        where TNumber : unmanaged, IBinaryInteger<TNumber>, IMinMaxValue<TNumber>, ISignedNumber<TNumber>
    {
        TNumber value = TNumber.Zero;

        Unsafe.CopyBlockUnaligned
        (
            destination: &value,
            source: Unsafe.AsPointer
            (
                ref Unsafe.Add
                (
                    source: ref MemoryMarshal.GetReference(this.CurrentTermContents),
                    elementOffset: 1
                )
            ),
            byteCount: (uint)this.CurrentTermContents.Length - 1
        );

        value &= TNumber.MaxValue;

        return MemoryMarshal.GetReference(this.CurrentTermContents) == 0 ? value : -value;
    }

#pragma warning disable CS8509 // switch expressions should handle every option; this one shouldn't

    /// <summary>
    /// Provides the implementation for performing a truncating read from an integer term.
    /// </summary>
    private readonly TNumber ReadInteger<TNumber>()
        where TNumber : IBinaryInteger<TNumber>
    {
        return this.TermType switch
        {
            TermType.Integer =>
                TNumber.CreateTruncating(BinaryPrimitives.ReadInt32BigEndian(this.CurrentTermContents)),

            TermType.SmallInteger =>
                TNumber.CreateTruncating((sbyte)MemoryMarshal.GetReference(this.CurrentTermContents))
        };
    }

#pragma warning restore CS8509

    /// <summary>
    /// Reads the current term as a signed byte.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadSByte
    (
        out sbyte value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 2)
                {
                    value = this.ReadSignedIntegerFromBigInteger<sbyte>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<sbyte>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as an unsigned byte.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadByte
    (
        out byte value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 2)
                {
                    value = this.ReadUnsignedIntegerFromBigInteger<byte>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<byte>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as a short.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadInt16
    (
        out short value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 3)
                {
                    value = this.ReadSignedIntegerFromBigInteger<short>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<short>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as a ushort.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadUInt16
    (
        out ushort value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 3)
                {
                    value = this.ReadUnsignedIntegerFromBigInteger<ushort>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<ushort>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as an int.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadInt32
    (
        out int value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 5)
                {
                    value = this.ReadSignedIntegerFromBigInteger<int>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<int>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as a uint.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadUInt32
    (
        out uint value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 5)
                {
                    value = this.ReadUnsignedIntegerFromBigInteger<uint>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<uint>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as a long.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadInt64
    (
        out long value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 9)
                {
                    value = this.ReadSignedIntegerFromBigInteger<long>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<long>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as a ulong.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadUInt64
    (
        out ulong value
    )
    {
        if (this.TermType.IsInteger())
        {
            if (this.TermType.IsBigInteger())
            {
                if (this.CurrentTermContents.Length >= 9)
                {
                    value = this.ReadUnsignedIntegerFromBigInteger<ulong>();
                    return true;
                }
            }
            else
            {
                value = this.ReadInteger<ulong>();
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as a BigInteger.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadBigInteger
    (
        out BigInteger value
    )
    {
        if (this.TermType.IsBigInteger())
        {
            value = this.ReadBigIntegerCore();
            return true;
        }
        else if (this.TermType.IsInteger())
        {
            value = new BigInteger(this.ReadInteger<int>());
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads the current term as an sbyte.
    /// </summary>
    public readonly sbyte ReadSByte()
    {
        if (this.TryReadSByte(out sbyte value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(sbyte));
        return default;
    }

    /// <summary>
    /// Reads the current term as a byte.
    /// </summary>
    public readonly byte ReadByte()
    {
        if (this.TryReadByte(out byte value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(byte));
        return default;
    }

    /// <summary>
    /// Reads the current term as a short.
    /// </summary>
    public readonly short ReadInt16()
    {
        if (this.TryReadInt16(out short value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(short));
        return default;
    }

    /// <summary>
    /// Reads the current term as a ushort.
    /// </summary>
    public readonly ushort ReadUInt16()
    {
        if (this.TryReadUInt16(out ushort value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(ushort));
        return default;
    }

    /// <summary>
    /// Reads the current term as an int.
    /// </summary>
    public readonly int ReadInt32()
    {
        if (this.TryReadInt32(out int value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(int));
        return default;
    }

    /// <summary>
    /// Reads the current term as a uint.
    /// </summary>
    public readonly uint ReadUInt32()
    {
        if (this.TryReadUInt32(out uint value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(uint));
        return default;
    }

    /// <summary>
    /// Reads the current term as a long.
    /// </summary>
    public readonly long ReadInt64()
    {
        if (this.TryReadInt64(out long value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(long));
        return default;
    }

    /// <summary>
    /// Reads the current term as a ulong.
    /// </summary>
    public readonly ulong ReadUInt64()
    {
        if (this.TryReadUInt64(out ulong value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(ulong));
        return default;
    }

    /// <summary>
    /// Reads the current term as a BigInteger.
    /// </summary>
    public readonly BigInteger ReadBigInteger()
    {
        if (this.TryReadBigInteger(out BigInteger value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(BigInteger));
        return default;
    }
}
