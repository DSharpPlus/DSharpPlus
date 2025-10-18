namespace DSharpPlus.Voice.RuntimeServices.Memory;

/// <summary>
/// Represents two signed 32-bit integers, used for a single audio sample. You should generally not use this type directly.
/// </summary>
public readonly struct Int32x2
{
    private readonly ulong value;

    public int First => (int)((this.value & 0xFFFFFFFF_00000000) >>> 32);
    public int Second => (int)(this.value & 0x00000000_FFFFFFFF);

    public Int32x2(int value)
        => this.value = (((ulong)(uint)value) << 32) | (uint)value;
}
