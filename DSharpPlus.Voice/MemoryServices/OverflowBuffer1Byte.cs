using System;
using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.MemoryServices;

internal struct OverflowBuffer1Byte
{
    private byte buffer;

    public int Available { get; private set; }

    public void SetOverflow(ReadOnlySpan<byte> overflow)
    {
        overflow.CopyTo(MemoryMarshal.CreateSpan(ref this.buffer, 1));
        this.Available = overflow.Length;
    }

    public void Clear()
        => this.Available = 0;

    public void CopyTo(Span<byte> target, out int written)
    {
        MemoryMarshal.CreateReadOnlySpan(ref this.buffer, this.Available).CopyTo(target);
        written = this.Available;

        this.Available = 0;
    }
}
