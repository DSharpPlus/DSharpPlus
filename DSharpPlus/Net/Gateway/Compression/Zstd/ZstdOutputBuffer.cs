namespace DSharpPlus.Net.Gateway.Compression.Zstd;

/// <summary>
/// Contains information about a buffer containing output from zstd.
/// </summary>
internal unsafe struct ZstdOutputBuffer
{
    internal byte* destination; // const void* dst
    internal nuint size; // size_t size
    internal nuint position; // size_t pos
}
