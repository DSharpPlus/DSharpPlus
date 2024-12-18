using System;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression;

/// <summary>
/// Represents a decompressor that doesn't decompress at all.
/// </summary>
public sealed class NullDecompressor : IPayloadDecompressor
{
    /// <inheritdoc/>
    public string? Name => null;

    // this decompressor *technically* applies transport-wide, and this simplifies composing the IDENTIFY payload.
    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        decompressed.Write(compressed);
        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {

    }

    /// <inheritdoc/>
    public void Reset()
    {

    }

    /// <inheritdoc/>
    public void Initialize()
    {

    }
}
