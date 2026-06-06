namespace DSharpPlus.Voice.Interop.Koana;

/// <summary>
/// Represents a vector passed to C#.
/// </summary>
internal unsafe struct NativeVector
{
    internal byte* data;
    internal int length;
    internal KoanaError error;
}
