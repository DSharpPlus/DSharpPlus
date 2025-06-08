namespace DSharpPlus.Voice.Interop.Aerith;

/// <summary>
/// Represents a possible error returned by libaeriths wrappers around STL containers.
/// </summary>
internal enum AerithWrapperError
{
    Success = 0,
    OutOfMemory = 1,
    LengthMismatch = 2
}
