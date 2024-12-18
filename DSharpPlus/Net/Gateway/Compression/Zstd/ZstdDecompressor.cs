using System;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zstd;

/// <summary>
/// A payload decompressor using zstd.
/// </summary>
public sealed class ZstdDecompressor : IPayloadDecompressor
{
    private ZstdInterop wrapper;

    /// <inheritdoc/>
    public string Name => "zstd-stream";

    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        // the magic header goes missing, we have to try it anyway - all explodes if we fix the magic header up :ioa:
        if (!this.wrapper.TryDecompress(compressed, decompressed))
        {
            decompressed.Clear();
            decompressed.Write(compressed);
        }

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.wrapper.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.wrapper.Dispose();
        this.wrapper = default;
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        if (this.wrapper != default)
        {
            Reset();
        }

        this.wrapper = new();
    }

    /// <summary>
    /// Frees the unmanaged zstd stream.
    /// </summary>
    ~ZstdDecompressor()
    {
        this.wrapper.Dispose();
    }
}
