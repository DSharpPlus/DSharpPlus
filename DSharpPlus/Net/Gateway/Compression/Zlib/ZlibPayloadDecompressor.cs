#pragma warning disable IDE0046

using System;

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
        using ZlibWrapper wrapper = new();

        if (!wrapper.TryInflate(compressed, decompressed))
        {
            decompressed.Clear();
            decompressed.Write(compressed);
        }

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {

    }
}
