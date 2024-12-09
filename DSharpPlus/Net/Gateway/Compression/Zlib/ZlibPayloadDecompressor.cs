#pragma warning disable IDE0046

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A payload decompressor using zlib on the payload level.
/// </summary>
public sealed class ZlibPayloadDecompressor : IPayloadDecompressor
{
    private static readonly Action<ReadOnlySpan<byte>, ArrayPoolBufferWriter<byte>> decompress = DecideDecompressionStrategy();

    /// <inheritdoc/>
    public string? Name => null;

    /// <inheritdoc/>
    public bool IsTransportCompression => false;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (BinaryPrimitives.ReadUInt16BigEndian(compressed) is not (0x7801 or 0x785E or 0x789C or 0x78DA))
        {
            decompressed.Write(compressed);
            return true;
        }

        decompress(compressed, decompressed);

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {

    }

    private static Action<ReadOnlySpan<byte>, ArrayPoolBufferWriter<byte>> DecideDecompressionStrategy()
    {
        if 
        (
            NativeLibrary.TryLoad("System.IO.Compression.Native", out nint handle) 
                && NativeLibrary.TryGetExport(handle, "CompressionNative_InflateInit2_", out _)
                && NativeLibrary.TryGetExport(handle, "CompressionNative_Inflate", out _)
                && NativeLibrary.TryGetExport(handle, "CompressionNative_InflateEnd", out _)
        )
        {
            return static (ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed) =>
            {
                using RuntimeBundledZlibBackend wrapper = new();
                wrapper.Inflate(compressed, decompressed);
            };
        }
        else
        {
            return static (ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed) =>
            {
                using ManagedZlibBackend wrapper = new();
                wrapper.Inflate(compressed, decompressed);
            };
        }
    }
}
