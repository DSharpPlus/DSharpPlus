namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// Used to signal to zlib how to behave in writing data to the output buffer.
/// </summary>
internal enum ZlibFlushCode : int
{
    SyncFlush = 2,
    Finish = 4,
}
