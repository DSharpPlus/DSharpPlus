// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using DSharpPlus.Etf.Objects;

namespace DSharpPlus.Etf.Serialization;

// here we deal with reading erlang-specific objects
// this doesn't need to be fast or anything, we just provide this for completeness
partial struct EtfReader
{
    /// <summary>
    /// Convenience utility to read atom terms from the current term at the given offset.
    /// </summary>
    /// <param name="offset">The offset to start at. This value will contain the final offset upon exit.</param>
    /// <param name="text">The read atom as string.</param>
    /// <returns>Whether the operation was successful.</returns>
    private readonly bool TryReadAtom
    (
        ref int offset,

        [NotNullWhen(true)]
        out string? text
    )
    {
        if (this.CurrentTermContents.Length < offset + 2)
        {
            text = null;
            return false;
        }

        ushort length = BinaryPrimitives.ReadUInt16BigEndian(this.CurrentTermContents[offset..2]);

        text = Encoding.UTF8.GetString(this.CurrentTermContents[(offset + 2)..length]);
        offset = length + 2;
        return true;
    }

    /// <summary>
    /// Reads the current term as a port term.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadPortTerm
    (
        out PortTerm term
    )
    {
        int offset = 0;

        if (this.TermType != TermType.Port)
        {
            term = default;
            return false;
        }

        if (!this.TryReadAtom(ref offset, out string? node))
        {
            term = default;
            return false;
        }

        if (this.CurrentTermContents.Length < offset + 5)
        {
            term = default;
            return false;
        }

        uint id = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[offset..4]);
        byte creation = this.CurrentTermContents[^1];

        term = new()
        {
            Node = node,
            Id = id,
            Creation = creation
        };

        return true;
    }

    /// <summary>
    /// Reads the current term as a new-port term.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadNewPortTerm
    (
        out NewPortTerm term
    )
    {
        int offset = 0;

        if (this.TermType != TermType.NewPort)
        {
            term = default;
            return false;
        }

        if (!this.TryReadAtom(ref offset, out string? node))
        {
            term = default;
            return false;
        }

        if (this.CurrentTermContents.Length < offset + 8)
        {
            term = default;
            return false;
        }

        uint id = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[offset..4]);
        uint creation = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[(offset + 4)..4]);

        term = new()
        {
            Node = node,
            Id = id,
            Creation = creation
        };

        return true;
    }

    /// <summary>
    /// Reads the current term as a v4-port term.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadV4PortTerm
    (
        out V4PortTerm term
    )
    {
        int offset = 0;

        if (this.TermType != TermType.NewPort)
        {
            term = default;
            return false;
        }

        if (!this.TryReadAtom(ref offset, out string? node))
        {
            term = default;
            return false;
        }

        if (this.CurrentTermContents.Length < offset + 12)
        {
            term = default;
            return false;
        }

        ulong id = BinaryPrimitives.ReadUInt64BigEndian(this.CurrentTermContents[offset..8]);
        uint creation = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[(offset + 8)..4]);

        term = new()
        {
            Node = node,
            Id = id,
            Creation = creation
        };

        return true;
    }
}
