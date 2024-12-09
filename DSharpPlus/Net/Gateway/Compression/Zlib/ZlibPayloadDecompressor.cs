using System;
using System.Buffers.Binary;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A payload decompressor using zlib on the payload level.
/// </summary>
public sealed class ZlibPayloadDecompressor : IPayloadDecompressor
{
    /// <inheritdoc/>
    public string? Name => null;

    /// <inheritdoc/>
    public bool IsTransportCompression => false;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (BinaryPrimitives.ReadUInt16BigEndian(compressed) is not (0x7801 or 0x785E or 0x789C or 0x78DA))
        {
            decompressed.Write(compressed);
            return true;
        }

        using RuntimeBundledZlibBackend wrapper = new();
        wrapper.Inflate(compressed, decompressed);

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {

    }
}
