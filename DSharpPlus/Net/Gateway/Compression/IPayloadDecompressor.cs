using System;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression;

/// <summary>
/// Contains functionality for decompressing inbound gateway payloads.
/// </summary>
public interface IPayloadDecompressor : IDisposable
{
    /// <summary>
    /// Gets the name of the decompressor.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Indicates whether the present compression format is connection-wide.
    /// </summary>
    public bool IsTransportCompression { get; }

    /// <summary>
    /// Attempts to decompress the provided payload.
    /// </summary>
    /// <param name="compressed">The raw, compressed data.</param>
    /// <param name="decompressed">A buffer writer for writing decompressed data.</param>
    /// <returns>A value indicating whether the operation was successful.</returns>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed);
}
