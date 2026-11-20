using System;
using System.Buffers;
using System.IO.Compression;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zstd;

/// <summary>
/// A payload decompressor using zstd.
/// </summary>
public sealed class ZstdDecompressor : IPayloadDecompressor
{
    private ZstandardDecoder decoder;
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

        OperationStatus status;

        do
        {
            status = this.decoder.Decompress(compressed, decompressed.GetSpan(), out int consumed, out int written);
            compressed = compressed[consumed..];
            decompressed.Advance(written);
        } 
        while (status == OperationStatus.DestinationTooSmall);

        // discord still strips the magic bytes, so NeedMoreData is (ideally) also successful. if it isn't it'll be caught by serialization throwing
        if (status is OperationStatus.Done or OperationStatus.NeedMoreData)
        {
            return true;
        }
        else
        {
            decompressed.Clear();
            return false;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.isInitialized)
        {
            this.decoder.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.decoder.Dispose();
        this.isInitialized = false;
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        if (this.isInitialized)
        {
            Reset();
        }

        this.decoder = new();
        this.isInitialized = true;
    }
}
