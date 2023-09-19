// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Buffers.Binary;
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
    *    
    ****************************************************************************************************
    * 
    * one note for the following section: the exact registers and moves may differ; the assembly here 
    * is taken from an (U)Int64 implementation
    * 
    ****************************************************************************************************
    *    
    * on x86, the emitted assembly output should look as follows for unsigned methods:
    * 
    * mov rax, bword ptr [rcx+0x40] | load the length of the current term as the reference passed
    *                               | to Unsafe.Add
    * ------------------------------|-------------------------------------------------------------------
    * mov rax, qword ptr [rax+0x01] | implicitly add and load the value at this address into rax, the
    *                               | return register
    * 
    ****************************************************************************************************
    * 
    * and for signed methods:
    * 
    * mov rax, bword ptr [rcx+0x40] | load the length of the current term as the reference passed
    *                               | to Unsafe.Add, but also as the reference we will use later to
    *                               | determine the sign
    * ------------------------------|-------------------------------------------------------------------
    * mov rcx, rax                  | move our work to rcx (or just any register that's currently
    *                               | free, it doesn't really matter) to keep the reference in rax
    *                               | intact
    * ------------------------------|-------------------------------------------------------------------
    * mov rdx, 0x7FFFFFFFFFFFFFFF   | set the maximum value up to ensure we don't break if the sign
    *                               | bit is already set
    * ------------------------------|-------------------------------------------------------------------
    * and rdx, qword ptr [rcx+0x01] | implicitly and and load the integer into rdx, the address
    *                               | incremented as Unsafe.Add instructed it to
    * ------------------------------|-------------------------------------------------------------------
    * mov rcx, rdx                  | copy the value into rcx to work there
    * ------------------------------|-------------------------------------------------------------------
    * neg rcx                       | negate the value in rcx, effectively `rcx = -rcx`. the JIT thinks
    *                               | rightly that it is faster to always negate than to take the
    *                               | branch, which hurts us by even existing
    * ------------------------------|-------------------------------------------------------------------
    * cmp byte ptr [rax], 0         | see whether the sign byte, with the reference stored in rax from
    *                               | the very beginning, equals zero
    * ------------------------------|-------------------------------------------------------------------
    * mov rax, rcx                  | move the negated value into rax to set up for evaluating the
    *                               | condition
    * ------------------------------|-------------------------------------------------------------------
    * cmove rax, rdx                | if the sign byte from earlier was zero, move the positive value
    *                               | into rax, if not, keep the negative value
    *
    * if the JIT insists on changing cmp byte ptr [rax], 0 to test byte ptr [rax], that's fine, any 
    * other changes should absolutely be analyzed for performance.
    * 
    ***************************************************************************************************/

    /// <summary>
    /// Provides the implementation for reading a unsigned integer from a currently provided
    /// BigInteger term, ignoring the sign byte.
    /// </summary>
    private readonly TNumber ReadUnsignedIntegerFromBigInteger<TNumber>()
        where TNumber : IUnsignedNumber<TNumber>, IBinaryInteger<TNumber>
    {
        return Unsafe.ReadUnaligned<TNumber>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents),
                elementOffset: 1
            )
        );
    }

    /// <summary>
    /// Provides the implementation for reading a signed integer from a currently provided
    /// BigInteger term.
    /// </summary>
    private readonly TNumber ReadSignedIntegerFromBigInteger<TNumber>()
        where TNumber : IBinaryInteger<TNumber>, IMinMaxValue<TNumber>, ISignedNumber<TNumber>
    {
        TNumber value = Unsafe.ReadUnaligned<TNumber>
        (
            ref Unsafe.Add
            (
                ref MemoryMarshal.GetReference(this.CurrentTermContents),
                elementOffset: 1
            )
        );

        value &= TNumber.MaxValue;

        return MemoryMarshal.GetReference(this.CurrentTermContents) == 0 ? value : -value;
    }

    /// <summary>
    /// Provides the implementation for performing a truncating read from an integer term.
    /// </summary>
    private readonly TNumber ReadFromIntegerTruncating<TNumber>()
        where TNumber : IBinaryInteger<TNumber>
    {
        int value = BinaryPrimitives.ReadInt32BigEndian(this.CurrentTermContents);

        return TNumber.CreateTruncating(value);
    }

    /// <summary>
    /// Provides the implementation for performing a saturating read from an integer term.
    /// </summary>
    private readonly TNumber ReadFromIntegerSaturating<TNumber>()
        where TNumber : IBinaryInteger<TNumber>
    {
        int value = BinaryPrimitives.ReadInt32BigEndian(this.CurrentTermContents);

        return TNumber.CreateSaturating(value);
    }

    /// <summary>
    /// Provides the implementation for performing a checked read from an integer term.
    /// </summary>
    private readonly TNumber ReadFromIntegerChecked<TNumber>()
        where TNumber : IBinaryInteger<TNumber>
    {
        int value = BinaryPrimitives.ReadInt32BigEndian(this.CurrentTermContents);

        return TNumber.CreateChecked(value);
    }

    /// <summary>
    /// Provides the implementation for performing a truncating read from a small-integer term.
    /// </summary>
    private readonly TNumber ReadFromByteTruncating<TNumber>()
        where TNumber : IBinaryInteger<TNumber> 
        => TNumber.CreateTruncating(MemoryMarshal.GetReference(this.CurrentTermContents));

    /// <summary>
    /// Provides the implementation for performing a saturating read from a small-integer term.
    /// </summary>
    private readonly TNumber ReadFromByteSaturating<TNumber>()
        where TNumber : IBinaryInteger<TNumber> 
        => TNumber.CreateSaturating(MemoryMarshal.GetReference(this.CurrentTermContents));

    /// <summary>
    /// Provides the implementation for performing a checked read from a small-integer term.
    /// </summary>
    private readonly TNumber ReadFromByteChecked<TNumber>()
        where TNumber : IBinaryInteger<TNumber> 
        => TNumber.CreateChecked(MemoryMarshal.GetReference(this.CurrentTermContents));
}
