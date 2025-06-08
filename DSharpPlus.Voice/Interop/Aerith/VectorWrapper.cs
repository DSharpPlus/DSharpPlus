namespace DSharpPlus.Voice.Interop.Aerith;

/// <summary>
/// A wrapper struct for <c>std::vector&lt;uint8_t&gt;</c> for C# to fetch the contents safely
/// </summary>
internal unsafe struct VectorWrapper
{
    internal byte* data;
    internal int length;
    internal AerithWrapperError error;
}
