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
    private bool isInitialized = false;

    /// <inheritdoc/>
    public string Name => "zstd-stream";

    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (!this.isInitialized)
        {
            return false;
        }

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
        if (this.isInitialized)
        {
            this.wrapper.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.wrapper.Dispose();
        this.isInitialized = false;
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        if (this.isInitialized)
        {
            Reset();
        }

        this.wrapper = new();
        this.isInitialized = true;
    }

    /// <summary>
    /// Frees the unmanaged zstd stream.
    /// </summary>
    ~ZstdDecompressor()
    {
        if (this.isInitialized)
        {
            this.wrapper.Dispose();
        }
    }
}
