// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Buffers.Binary;

namespace DSharpPlus.Etf.Serialization;

/*******************************************************************************************************
* here, we deal with preliminary lexing and length decoding of the next term ahead
*
* the pattern for all methods here is `int LexName(out bool success);` returning the length looked at
* and writing whether the procedure was successful to an out parameter. this enables somewhat cleaner
* aggregation in EtfReader.Read while not producing slower assembly, despite breaking convention of
* `bool TryLexName(out int length);`
*
* furthermore, in methods where there are only very few if statements with short bodies, the success
* path should be inside the first if tree, never branching to an - explicit or implicit - else.
* this is specifically designed to make branch predictors happy; from RyuJIT to the predictors in most
* CPUs, branch predictors usually expect the 'hottest'/most common path to be in the first if tree.
* this especially matters on apple silicon, where prediction failures are extremely expensive, and on
* intel x86 CPUs.
* in T1, the JIT generally picks up on it, but there's a throwhelper call associated with ROS.Slice that
* still causes marginal losses in T1, and much less marginal losses in the failure branch.
*******************************************************************************************************/

partial struct EtfReader
{
    /// <summary>
    /// Lexes an atom-cache term, as per erl_ext_dist 4.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexAtomCache
    (
        out bool success
    )
    {
        if (this.index + 1 <= this.data.Length)
        {
            this.CurrentTermContents = this.data.Slice(this.index, 1);

            success = true;
            return 1;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes an atom-utf8 term, as per erl_ext_dist 30.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexAtomUtf8
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            ushort length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            if (this.index + 2 + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index + 2, length);

                success = true;
                return length + 2;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a binary term, as per erl_ext_dist 19.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexBinary
    (
        out bool success
    )
    {
        if (this.index + 4 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 4);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(lengthSlice);

            if (this.index + 4 + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index + 4, (int)length);

                success = true;
                return (int)length + 4;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a bit-binary term, as per erl_ext_dist 28.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexBitBinary
    (
        out bool success
    )
    {
        if (this.index + 4 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 4);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(lengthSlice);

            // we do this by simply prepending the raw span with the amount of valid bits in the last byte
            if (this.index + 5 + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index + 4, (int)length + 1);

                success = true;
                return (int)length + 5;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes an export term, as per erl_ext_dist 27.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexExport
    (
        out bool success
    )
    {
        // first atom: module name
        if (this.index + 2 <= this.data.Length)
        {
            int accumulatedLength = 2;

            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            ushort length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            accumulatedLength += length;

            // second atom: function name
            if (this.index + accumulatedLength + 2 <= this.data.Length)
            {
                lengthSlice = this.data.Slice(this.index + accumulatedLength, 2);
                length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

                accumulatedLength += 2 + length;

                // one byte for arity
                if (this.index + ++accumulatedLength <= this.data.Length)
                {
                    this.CurrentTermContents = this.data.Slice(this.index, accumulatedLength);

                    success = true;
                    return accumulatedLength;
                }
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a float term, as per erl_ext_dist 7.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexFloat
    (
        out bool success
    )
    {
        if (this.index + 31 <= this.data.Length)
        {
            this.CurrentTermContents = this.data.Slice(this.index, 31);

            success = true;
            return 31;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes an integer term, as per erl_ext_dist 6.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexInteger
    (
        out bool success
    )
    {
        if (this.index + 4 <= this.data.Length)
        {
            this.CurrentTermContents = this.data.Slice(this.index, 4);

            success = true;
            return 4;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a large big-integer term, as per erl_ext_dist 21.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexLargeBigInteger
    (
        out bool success
    )
    {
        if (this.index + 4 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 4);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(lengthSlice);

            // the first byte here carries the sign
            if (this.index + 5 + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index + 4, (int)length + 1);

                success = true;
                return (int)length + 5;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a large tuple term, as per erl_ext_dist 14.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private readonly int LexLargeTuple
    (
        out bool success
    )
    {
        if (this.index + 4 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 4);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(lengthSlice);

            this.complexObjects.Push(TermType.LargeTuple);
            this.remainingLengths.Push(length);

            success = true;
            return 4;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a list term, as per erl_ext_dist 18.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private readonly int LexList
    (
        out bool success
    )
    {
        if (this.index + 4 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 4);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(lengthSlice);

            this.complexObjects.Push(TermType.LargeTuple);

            // account for the null-terminator-that-may-not-actually-be-null-at-all
            this.remainingLengths.Push(length + 1);

            success = true;
            return 4;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a local term, as per erl_ext_dist 34.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private static int LexLocal
    (
        out bool success
    )
    {
        success = true;
        return 0;
    }

    /// <summary>
    /// Lexes a map term, as per erl_ext_dist 15.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private readonly int LexMap
    (
        out bool success
    )
    {
        if (this.index + 4 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 4);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(lengthSlice);

            this.complexObjects.Push(TermType.LargeTuple);
            this.remainingLengths.Push(length * 2);

            success = true;
            return 4;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a newer-reference term, as per erl_ext_dist 24.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexNewerReference
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            ushort idLength = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            // decode the atom node name
            if (this.index + 4 <= this.data.Length)
            {
                lengthSlice = this.data.Slice(this.index + 2, 2);
                ushort nodeLength = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

                int accumulatedLength = (idLength * 4) + nodeLength + 8;

                if (this.index + accumulatedLength <= this.data.Length)
                {
                    this.CurrentTermContents = this.data.Slice(this.index, accumulatedLength);

                    success = true;
                    return accumulatedLength;
                }
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a new-float term, as per erl_ext_dist 29.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexNewFloat
    (
        out bool success
    )
    {
        if (this.index + 8 <= this.data.Length)
        {
            this.CurrentTermContents = this.data.Slice(this.index, 8);

            success = true;
            return 8;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a new-PID term, as per erl_ext_dist 12.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexNewPid
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {

            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            int length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            length += 14;

            if (this.index + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index, length);
                
                success = true;
                return length;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a new-port term, as per erl_ext_dist 9.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexNewPort
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {

            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            int length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            length += 10;

            if (this.index + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index, length);

                success = true;
                return length;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a new-reference term, as per erl_ext_dist 23.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexNewReference
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            ushort idLength = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            // decode the atom node name
            if (this.index + 4 <= this.data.Length)
            {
                lengthSlice = this.data.Slice(this.index + 2, 2);
                ushort nodeLength = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

                int accumulatedLength = (idLength * 4) + nodeLength + 5;

                if (this.index + accumulatedLength <= this.data.Length)
                {
                    this.CurrentTermContents = this.data.Slice(this.index, accumulatedLength);

                    success = true;
                    return accumulatedLength;
                }
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a nil term, as per erl_ext_dist 16.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private static int LexNil
    (
        out bool success
    )
    {
        success = true;
        return 0;
    }

    /// <summary>
    /// Lexes a PID term, as per erl_ext_dist 11.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexPid
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {

            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            int length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            length += 11;

            if (this.index + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index, length);

                success = true;
                return length;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a port term, as per erl_ext_dist 8.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexPort
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {

            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            int length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            length += 7;

            if (this.index + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index, length);

                success = true;
                return length;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes an atom-utf8 term, as per erl_ext_dist 30.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexSmallAtomUtf8
    (
        out bool success
    )
    {
        if (this.index + 1 <= this.data.Length)
        {
            byte length = this.data[this.index + 1];

            if (this.index + 1 + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index + 1, length);

                success = true;
                return length + 1;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a small big-integer term, as per erl_ext_dist 21.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexSmallBigInteger
    (
        out bool success
    )
    {
        if (this.index + 1 <= this.data.Length)
        {
            byte length = this.data[this.index];

            // the first byte here carries the sign
            if (this.index + 2 + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index + 1, (int)length + 1);

                success = true;
                return (int)length + 5;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a small integer term, as per erl_ext_dist 5.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexSmallInteger
    (
        out bool success
    )
    {
        if (this.index + 1 <= this.data.Length)
        {
            this.CurrentTermContents = this.data.Slice(this.index, 1);

            success = true;
            return 1;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a small tuple term, as per erl_ext_dist 13.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private readonly int LexSmallTuple
    (
        out bool success
    )
    {
        if (this.index + 1 <= this.data.Length)
        {
            byte length = this.data[this.index];

            this.complexObjects.Push(TermType.SmallTuple);
            this.remainingLengths.Push(length);

            success = true;
            return 1;
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a string term, as per erl_ext_dist 17.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexString
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {
            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            ushort length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            if (this.index + 2 + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index + 2, length);

                success = true;
                return length + 2;
            }
        }

        success = false;
        return 0;
    }

    /// <summary>
    /// Lexes a v4 port term, as per erl_ext_dist 10.
    /// </summary>
    /// <param name="success">Indicates whether the term was read successfully.</param>
    /// <returns>The amount of bytes processed.</returns>
    private int LexV4Port
    (
        out bool success
    )
    {
        if (this.index + 2 <= this.data.Length)
        {

            ReadOnlySpan<byte> lengthSlice = this.data.Slice(this.index, 2);
            int length = BinaryPrimitives.ReadUInt16BigEndian(lengthSlice);

            length += 14;

            if (this.index + length <= this.data.Length)
            {
                this.CurrentTermContents = this.data.Slice(this.index, length);

                success = true;
                return length;
            }
        }

        success = false;
        return 0;
    }
}
