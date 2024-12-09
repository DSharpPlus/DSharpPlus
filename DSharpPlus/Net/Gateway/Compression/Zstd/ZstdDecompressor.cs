using System;
using System.Buffers.Binary;

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
        if (BinaryPrimitives.ReadUInt32LittleEndian(compressed) is not 0xFD2FB528)
        {
            decompressed.Write(compressed);
            return true;
        }

        this.wrapper.Decompress(compressed, decompressed);

        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => this.wrapper.Dispose();
}
