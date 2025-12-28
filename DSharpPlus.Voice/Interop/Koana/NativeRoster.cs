namespace DSharpPlus.Voice.Interop.Koana;

/// <summary>
/// Represents a roster of users and their encryption keys passed to C#.
/// </summary>
internal unsafe struct NativeRoster
{
    internal ulong* keys;
    internal byte** values;
    internal int* valueLengths;
    internal int length;
    internal KoanaError error;
}
