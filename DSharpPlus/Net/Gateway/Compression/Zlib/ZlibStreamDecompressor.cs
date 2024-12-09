#pragma warning disable IDE0046

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A payload decompressor using zlib on the transport level.
/// </summary>
public sealed class ZlibStreamDecompressor : IPayloadDecompressor
{
    private RuntimeBundledZlibBackend wrapper = new();
    private static readonly Action<ZlibStreamDecompressor, ReadOnlySpan<byte>, ArrayPoolBufferWriter<byte>> decompress = DecideDecompressionStrategy();

    /// <inheritdoc/>
    public string Name => "zlib-stream";

    /// <inheritdoc/>
    public bool IsTransportCompression => true;

    /// <inheritdoc/>
    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (BinaryPrimitives.ReadUInt16BigEndian(compressed) is not (0x7801 or 0x785E or 0x789C or 0x78DA))
        {
            decompressed.Write(compressed);
            return true;
        }

        decompress(this, compressed, decompressed);

        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => this.wrapper.Dispose();

    private static Action<ZlibStreamDecompressor, ReadOnlySpan<byte>, ArrayPoolBufferWriter<byte>> DecideDecompressionStrategy()
    {
        if
        (
            NativeLibrary.TryLoad("System.IO.Compression.Native", out nint handle)
                && NativeLibrary.TryGetExport(handle, "CompressionNative_InflateInit2_", out _)
                && NativeLibrary.TryGetExport(handle, "CompressionNative_Inflate", out _)
                && NativeLibrary.TryGetExport(handle, "CompressionNative_InflateEnd", out _)
        )
        {
            return static (ZlibStreamDecompressor decompressor, ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed) 
                => decompressor.wrapper.Inflate(compressed, decompressed);
        }
        else
        {
            return static (ZlibStreamDecompressor decompressor, ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed) 
                => decompressor.wrapper.Inflate(compressed, decompressed);
        }
    }
}
