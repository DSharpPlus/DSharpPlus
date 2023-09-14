// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using Bundles.ValueCollections;

namespace DSharpPlus.Etf.Serialization;

/// <summary>
/// Provides a high-performance, low-allocation API for forward-only, read-only access to Erlang External
/// Term Format, version 131, encoded data. It processes binary data sequentially with no caching and
/// without support for non-standard data or older format versions, according to the specification at
/// <seealso href="https://www.erlang.org/doc/apps/erts/erl_ext_dist.html"/>.
/// This type does not support asynchronous reading nor reentrancy.
/// </summary>
/// <remarks>
/// This type expects provided data to be uncompressed, that is, to not start with 0x8350.
/// </remarks>
public ref partial struct EtfReader
{
    /***************************************************************************************************
    * do not change field order without accounting for struct layout -- this is an unmanaged struct;
    * layout is explicit here!
    * 
    * the current layout is putting the ValueStacks first, which are 24b each; then the spans, which
    * are 16b each, then an integer and several byte-sized fields. this leads to 87 bytes used and 1
    * byte padding required on 8-byte-aligned ABIs, 9 bytes padding on higher alignment.
    * 
    * copying this struct will always split at least one cache line, so passing by ref is advised 
    * where possible - which should be most cases.
    ***************************************************************************************************/

    private readonly ValueStack<uint> remainingLengths;
    private readonly ValueStack<TermType> complexObjects;

    private readonly ReadOnlySpan<byte> data;
    private ReadOnlySpan<byte> rawTerm;

    private int index;
    private TermType currentTerm;
    private EtfTokenType previousToken;
    private EtfTokenType currentToken;

    /// <summary>
    /// Constructs a new <seealso cref="EtfReader"/> with a maximum depth of 256.
    /// </summary>
    /// <param name="data">The span containing the binary ETF to process.</param>
    public EtfReader
    (
        ReadOnlySpan<byte> data
    )
        : this(data, 256)
    {

    }

    /// <summary>
    /// Constructs a new <seealso cref="EtfReader"/> with the specified maximum depth.
    /// </summary>
    /// <param name="data">The span containing the binary ETF to process.</param>
    /// <param name="maxDepth">The maximum nesting depth for maps and lists.</param>
    /// <exception cref="ArgumentException">Thrown if the root term was compressed.</exception>
    public EtfReader
    (
        ReadOnlySpan<byte> data,
        int maxDepth
    )
        : this
        (
            data,
            new(new uint[maxDepth]),
            new(new TermType[maxDepth])
        )
    {

    }

    /// <summary>
    /// Constructs a new <seealso cref="EtfReader"/>, using the specified buffers to store temporary metadata.
    /// </summary>
    /// <remarks>
    /// It is legal for call-sites to pass stackallocated ValueStack instances here, thereby eliminating two
    /// array allocations. This is, however, illegal if the EtfReader is then passed down the stack, in which
    /// case memory it depends on will be destroyed as part of the stack frame.
    /// </remarks>
    /// <param name="data">The span containing the binary ETF to process.</param>
    /// <param name="lengths">A stack-buffer for remaining object lengths.</param>
    /// <param name="objects">A stack-buffer for all complex objects.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the capacities of the two buffers do not match, or if the root term was compressed.
    /// </exception>
    public EtfReader
    (
        ReadOnlySpan<byte> data,
        ValueStack<uint> lengths,
        ValueStack<TermType> objects
    )
    {
        if (lengths.Capacity != objects.Capacity)
        {
            throw new ArgumentException("Inconsistent maximum depth between object lengths and object types.");
        }

        if (this.data[0] != 0x83)
        {
            throw new ArgumentException("The data was provided in a wrong format or format version.");
        }

        if (this.data[1] == 0x50)
        {
            throw new ArgumentException("Compressed data cannot be processed.");
        }

        this.data = data;
        this.remainingLengths = lengths;
        this.complexObjects = objects;
        this.index = 1;
    }

    /// <summary>
    /// Reads the next ETF term from the input source.
    /// </summary>
    /// <returns>True if the term was read successfully.</returns>
    public bool Read()
    {
        if (this.index + 1 == this.data.Length)
        {
            return false;
        }

        // ugly ETF hacks :3
        // because ETF doesn't have end tokens (luckily!), we synthesize them here to be able to expose
        // an acceptable API without performance sacrifices
        // we also, importantly, do this BEFORE decoding the next term.
        if (this.remainingLengths.Count != 0 && this.remainingLengths.Peek() == 0)
        {
            this.currentToken = this.complexObjects.Pop() switch
            {
                TermType.Map => EtfTokenType.EndMap,
                TermType.List => EtfTokenType.EndList,
                TermType.SmallTuple or TermType.LargeTuple => EtfTokenType.EndTuple,
                _ => EtfTokenType.Term
            };

            this.remainingLengths.Pop();
            return true;
        }

        TermType term = (TermType)this.data[this.index++];

        this.currentTerm = term;
        this.previousToken = this.currentToken;
        this.currentToken = term switch
        {
            TermType.Map => EtfTokenType.StartMap,
            TermType.List => EtfTokenType.StartList,
            TermType.SmallTuple or TermType.LargeTuple => EtfTokenType.StartTuple,
            _ => EtfTokenType.Term
        };

        bool success = false;

        // decrease lengths before potentially pushing a new length
        if (this.complexObjects.Count != 0)
        {
            scoped ref uint currentLength = ref this.remainingLengths.PeekRef();
            currentLength--;
        }

        this.index += term switch
        {
            TermType.AtomCache => LexAtomCache(out success),
            TermType.AtomUtf8 => LexAtomUtf8(out success),
            TermType.Binary => LexBinary(out success),
            TermType.BitBinary => LexBitBinary(out success),
            TermType.Export => LexExport(out success),
            TermType.Float => LexFloat(out success),
            TermType.Integer => LexInteger(out success),
            TermType.LargeBig => LexLargeBigInteger(out success),
            TermType.LargeTuple => LexLargeTuple(out success),
            TermType.List => LexList(out success),
            TermType.Local => LexLocal(out success),
            TermType.Map => LexMap(out success),
            TermType.NewerReference => LexNewerReference(out success),
            TermType.NewFloat => LexNewFloat(out success),
            TermType.NewPid => LexNewPid(out success),
            TermType.NewPort => LexNewPort(out success),
            TermType.NewReference => LexNewReference(out success),
            TermType.Nil => LexNil(out success),
            TermType.Pid => LexPid(out success),
            TermType.Port => LexPort(out success),
            TermType.SmallAtomUtf8 => LexSmallAtomUtf8(out success),
            TermType.SmallBig => LexSmallBigInteger(out success),
            TermType.SmallInteger => LexSmallInteger(out success),
            TermType.SmallTuple => LexSmallTuple(out success),
            TermType.String => LexString(out success),
            TermType.V4Port => LexV4Port(out success),
            _ => 0
        };

        return success;
    }
}
