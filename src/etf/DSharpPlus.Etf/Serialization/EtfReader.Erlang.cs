// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Buffers.Binary;
using System.Collections.Generic;
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

        if (this.TermType != TermType.V4Port)
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

    /// <summary>
    /// Reads the current term as a PID term.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadPidTerm
    (
        out PidTerm term
    )
    {
        int offset = 0;

        if (this.TermType != TermType.Pid)
        {
            term = default;
            return false;
        }

        if (!this.TryReadAtom(ref offset, out string? node))
        {
            term = default;
            return false;
        }

        if (this.CurrentTermContents.Length < offset + 9)
        {
            term = default;
            return false;
        }

        uint id = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[offset..4]);
        uint serial = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[(offset + 4)..4]);
        byte creation = this.CurrentTermContents[^1];

        term = new()
        {
            Node = node,
            Id = id,
            Serial = serial,
            Creation = creation
        };

        return true;
    }

    /// <summary>
    /// Reads the current term as a PID term.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadNewPidTerm
    (
        out NewPidTerm term
    )
    {
        int offset = 0;

        if (this.TermType != TermType.NewPid)
        {
            term = default;
            return false;
        }

        if (!this.TryReadAtom(ref offset, out string? node))
        {
            term = default;
            return false;
        }

        if (this.CurrentTermContents.Length < offset + 9)
        {
            term = default;
            return false;
        }

        uint id = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[offset..4]);
        uint serial = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[(offset + 4)..4]);
        uint creation = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[(offset + 8)..4]);

        term = new()
        {
            Node = node,
            Id = id,
            Serial = serial,
            Creation = creation
        };

        return true;
    }

    /// <summary>
    /// Reads the current term as a new reference term.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadNewReferenceTerm
    (
        out NewReferenceTerm term
    )
    {
        int offset = 2;

        if (this.TermType != TermType.NewReference)
        {
            term = default;
            return false;
        }

        ushort length = BinaryPrimitives.ReadUInt16BigEndian(this.CurrentTermContents[..2]);

        if (!this.TryReadAtom(ref offset, out string? node))
        {
            term = default;
            return false;
        }

        if (this.CurrentTermContents.Length < offset + (length * 4) + 1)
        {
            term = default;
            return false;
        }

        byte creation = this.CurrentTermContents[offset++];

        List<uint> id = new(length);

        for (int i = 0; i < length; i++)
        {
            id.Add(BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[offset..4]));
            offset += 4;
        }

        term = new()
        {
            Node = node,
            Creation = creation,
            Id = id
        };

        return true;
    }

    /// <summary>
    /// Reads the current term as a newer reference term.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadNewerReferenceTerm
    (
        out NewerReferenceTerm term
    )
    {
        int offset = 2;

        if (this.TermType != TermType.NewReference)
        {
            term = default;
            return false;
        }

        ushort length = BinaryPrimitives.ReadUInt16BigEndian(this.CurrentTermContents[..2]);

        if (!this.TryReadAtom(ref offset, out string? node))
        {
            term = default;
            return false;
        }

        if (this.CurrentTermContents.Length < offset + (length * 4) + 4)
        {
            term = default;
            return false;
        }

        uint creation = BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[offset..4]);
        offset += 4;

        List<uint> id = new(length);

        for (int i = 0; i < length; i++)
        {
            id.Add(BinaryPrimitives.ReadUInt32BigEndian(this.CurrentTermContents[offset..4]));
            offset += 4;
        }

        term = new()
        {
            Node = node,
            Creation = creation,
            Id = id
        };

        return true;
    }

    /// <summary>
    /// Reads the current term as port term.
    /// </summary>
    public readonly PortTerm ReadPortTerm()
    {
        if (this.TryReadPortTerm(out PortTerm term))
        {
            return term;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(PortTerm));
        return default;
    }

    /// <summary>
    /// Reads the current term as new port term.
    /// </summary>
    public readonly NewPortTerm ReadNewPortTerm()
    {
        if (this.TryReadNewPortTerm(out NewPortTerm term))
        {
            return term;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(NewPortTerm));
        return default;
    }

    /// <summary>
    /// Reads the current term as v4 port term.
    /// </summary>
    public readonly V4PortTerm ReadV4PortTerm()
    {
        if (this.TryReadV4PortTerm(out V4PortTerm term))
        {
            return term;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(V4PortTerm));
        return default;
    }

    /// <summary>
    /// Reads the current term as PID term.
    /// </summary>
    public readonly PidTerm ReadPidTerm()
    {
        if (this.TryReadPidTerm(out PidTerm term))
        {
            return term;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(PidTerm));
        return default;
    }

    /// <summary>
    /// Reads the current term as new PID term.
    /// </summary>
    public readonly NewPidTerm ReadNewPidTerm()
    {
        if (this.TryReadNewPidTerm(out NewPidTerm term))
        {
            return term;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(NewPidTerm));
        return default;
    }

    /// <summary>
    /// Reads the current term as new reference term.
    /// </summary>
    public readonly NewReferenceTerm ReadNewReferenceTerm()
    {
        if (this.TryReadNewReferenceTerm(out NewReferenceTerm term))
        {
            return term;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(NewReferenceTerm));
        return default;
    }

    /// <summary>
    /// Reads the current term as newer reference term.
    /// </summary>
    public readonly NewerReferenceTerm ReadNewerReferenceTerm()
    {
        if (this.TryReadNewerReferenceTerm(out NewerReferenceTerm term))
        {
            return term;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(NewerReferenceTerm));
        return default;
    }
}
