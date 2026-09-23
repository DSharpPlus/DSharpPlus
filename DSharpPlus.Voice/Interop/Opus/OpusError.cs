namespace DSharpPlus.Voice.Interop.Opus;

/// <summary>
/// Represents a native error returned by opus.
/// </summary>
internal enum OpusError
{
    OpusOK = 0,
    OpusBadArgument = -1,
    OpusBufferTooSmall = -2,
    OpusInternalError = -3,
    OpusInvalidPacket = -4,
    OpusUnimplemented = -5,
    OpusInvalidState = -6,
    OpusAllocationFailure = -7
}
