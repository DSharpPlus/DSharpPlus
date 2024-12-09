using System;
using System.Buffers.Binary;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A payload decompressor using zlib on the transport level.
/// </summary>
public sealed class ZlibStreamDecompressor : IPayloadDecompressor
{
    private RuntimeBundledZlibBackend wrapper = new();

    /// <inheritdoc/>
    public string Name => "zlib-stream";

    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (BinaryPrimitives.ReadUInt16BigEndian(compressed) is not (0x7801 or 0x785E or 0x789C or 0x78DA))
        {
            decompressed.Write(compressed);
            return true;
        }

        this.wrapper.Inflate(compressed, decompressed);

        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => this.wrapper.Dispose();
}
