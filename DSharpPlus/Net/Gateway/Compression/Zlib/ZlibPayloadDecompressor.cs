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
        if (BinaryPrimitives.ReadUInt32BigEndian(compressed[^4..]) is not 0x0000FFFF)
        {
            decompressed.Write(compressed);
            return true;
        }

        using ZlibInterop wrapper = new();

        while (true)
        {
            bool complete = wrapper.Inflate
            (
                compressed,
                decompressed.GetSpan(),
                out int written
            );

            decompressed.Advance(written);

            if (complete)
            {
                break;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {

    }
}
