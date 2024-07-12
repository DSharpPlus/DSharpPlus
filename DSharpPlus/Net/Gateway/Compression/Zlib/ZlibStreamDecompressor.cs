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
    private ZlibInterop wrapper = new();

    /// <inheritdoc/>
    public string Name => "zlib-stream";

    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (BinaryPrimitives.ReadUInt32BigEndian(compressed[^4..]) is not 0x0000FFFF)
        {
            decompressed.Write(compressed);
            return true;
        }

        while (true)
        {
            bool complete = this.wrapper.Inflate
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
    public void Dispose() => this.wrapper.Dispose();
}
