
using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

namespace DSharpPlus.Net.WebSocket;
internal sealed class PayloadDecompressor : IDisposable
{
    private const uint ZlibFlush = 0x0000FFFF;
    private const byte ZlibPrefix = 0x78;

    public GatewayCompressionLevel CompressionLevel { get; }

    private MemoryStream CompressedStream { get; }
    private DeflateStream DecompressorStream { get; }

    public PayloadDecompressor(GatewayCompressionLevel compressionLevel)
    {
        if (compressionLevel == GatewayCompressionLevel.None)
        {
            throw new InvalidOperationException("Decompressor requires a valid compression mode.");
        }

        CompressionLevel = compressionLevel;
        CompressedStream = new MemoryStream();
        if (CompressionLevel == GatewayCompressionLevel.Stream)
        {
            DecompressorStream = new DeflateStream(CompressedStream, CompressionMode.Decompress);
        }
    }

    public bool TryDecompress(ArraySegment<byte> compressed, MemoryStream decompressed)
    {
        DeflateStream zlib = CompressionLevel == GatewayCompressionLevel.Stream
            ? DecompressorStream
            : new DeflateStream(CompressedStream, CompressionMode.Decompress, true);

        if (compressed.Array[0] == ZlibPrefix)
        {
            CompressedStream.Write(compressed.Array, compressed.Offset + 2, compressed.Count - 2);
        }
        else
        {
            CompressedStream.Write(compressed.Array, compressed.Offset, compressed.Count);
        }

        CompressedStream.Flush();
        CompressedStream.Position = 0;

        Span<byte> cspan = compressed.AsSpan();
        uint suffix = BinaryPrimitives.ReadUInt32BigEndian(cspan[^4..]);
        if (CompressionLevel == GatewayCompressionLevel.Stream && suffix != ZlibFlush)
        {
            if (CompressionLevel == GatewayCompressionLevel.Payload)
            {
                zlib.Dispose();
            }

            return false;
        }

        try
        {
            zlib.CopyTo(decompressed);
            return true;
        }
        catch { return false; }
        finally
        {
            CompressedStream.Position = 0;
            CompressedStream.SetLength(0);

            if (CompressionLevel == GatewayCompressionLevel.Payload)
            {
                zlib.Dispose();
            }
        }
    }

    public void Dispose()
    {
        DecompressorStream?.Dispose();
        CompressedStream.Dispose();
    }
}
