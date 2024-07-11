using System;

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
