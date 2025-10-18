using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

internal struct OverflowBuffer7Bytes
{
    private Buffer7 buffer;

    public int Available { get; private set; }

    public void SetOverflow(ReadOnlySpan<byte> overflow)
    {
        overflow.CopyTo(MemoryMarshal.CreateSpan(ref this.buffer.value, 7));
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

    [InlineArray(7)]
    private struct Buffer7
    {
        public byte value;
    }
}
