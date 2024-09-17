namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// Indicates a return/error code from zlib.
/// </summary>
internal enum ZlibErrorCode
{
    /// <summary>
    /// Everything is fine.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Everything is fine and we completed the stream.
    /// </summary>
    StreamEnd = 1,

    /// <summary>
    /// The stream was in an invalid state.
    /// </summary>
    StreamError = -2,

    /// <summary>
    /// The data we had was malformed.
    /// </summary>
    DataError = -3,

    /// <summary>
    /// We ran out of memory.
    /// </summary>
    MemoryError = -4,

    /// <summary>
    /// The output buffer didn't have enough space.
    /// </summary>
    BufferError = -5,

    /// <summary>
    /// We encountered a zlib version mismatch.
    /// </summary>
    VersionError = -6
}
