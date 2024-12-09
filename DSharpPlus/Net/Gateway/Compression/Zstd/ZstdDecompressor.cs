using System;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zstd;

/// <summary>
/// A payload decompressor using zstd.
/// </summary>
public sealed class ZstdDecompressor : IPayloadDecompressor
{
    private ZstdInterop wrapper = new();

    /// <inheritdoc/>
    public string Name => "zstd-stream";

    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        // the magic header goes missing, we have to try it anyway - all explodes if we fix the magic header up :ioa:
        if (this.wrapper.TryDecompress(compressed, decompressed))
        {
            return true;
        }

        decompressed.Write(compressed);
        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => this.wrapper.Dispose();
}
