namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// Represents two signed 16-bit integers, used for a single audio sample. You should generally not use this type directly.
/// </summary>
public readonly struct Int16x2
{
    private readonly int value;

    public short First => (short)((this.value & 0xFFFF_0000) >>> 16);
    public short Second => (short)(this.value & 0x0000_FFFF);

    public Int16x2(short value)
        => this.value = (((int)(ushort)value) << 16) | (ushort)value;
}
