using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal struct OverflowBuffer3Bytes
{
    private Buffer3 buffer;

    public int Available { get; private set; }

    public void SetOverflow(ReadOnlySpan<byte> overflow)
    {
        overflow.CopyTo(this.buffer);
        this.Available = overflow.Length;
    }

    public void Clear()
        => this.Available = 0;

    public void CopyTo(Span<byte> target, out int written)
    {
        MemoryMarshal.CreateReadOnlySpan(ref this.buffer.value, this.Available).CopyTo(target);
        written = this.Available;

        this.Available = 0;
    }

    [InlineArray(3)]
    private struct Buffer3
    {
        public byte value;
    }
}
