namespace DSharpPlus.Net.Gateway.Compression.Zstd;

/// <summary>
/// Contains information about a buffer passed as input to zstd.
/// </summary>
internal unsafe struct ZstdInputBuffer
{
    internal byte* source; // const void* src
    internal nuint size; // size_t size
    internal nuint position; // size_t pos
}
