namespace DSharpPlus.Voice.Interop.Aerith;

/// <summary>
/// A wrapper struct for a a <code>RosterMap</code>.
/// </summary>
internal unsafe struct RosterWrapper
{
    internal ulong* keys;
    internal byte** values;
    internal int* valueLengths;
    internal int length;
    internal AerithWrapperError error;
}
