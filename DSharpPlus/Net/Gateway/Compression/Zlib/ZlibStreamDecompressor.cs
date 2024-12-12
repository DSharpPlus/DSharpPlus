#pragma warning disable IDE0046

using System;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A payload decompressor using zlib on the transport level.
/// </summary>
public sealed class ZlibStreamDecompressor : IPayloadDecompressor
{
    private readonly ZlibWrapper wrapper = new();

    /// <inheritdoc/>
    public string Name => "zlib-stream";

    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (!this.wrapper.TryInflate(compressed, decompressed))
        {
            decompressed.Clear();
            decompressed.Write(compressed);
        }

        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => this.wrapper.Dispose();
}
