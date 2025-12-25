namespace DSharpPlus.Voice.MemoryServices;

/// <summary>
/// Represents two signed 32-bit integers, used for a single audio sample. You should generally not use this type directly.
/// </summary>
public readonly struct Singlex2
{
    private readonly float value0;
    private readonly float value1;

    public float First => this.value0;
    public float Second => this.value1;

    public Singlex2(float value)
    {
        this.value0 = value;
        this.value1 = value;
    }
}
