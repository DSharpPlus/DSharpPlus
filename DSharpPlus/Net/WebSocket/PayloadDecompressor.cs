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

        this.CompressionLevel = compressionLevel;
        this.CompressedStream = new MemoryStream();
        if (this.CompressionLevel == GatewayCompressionLevel.Stream)
        {
            this.DecompressorStream = new DeflateStream(this.CompressedStream, CompressionMode.Decompress);
        }
    }

    public bool TryDecompress(ArraySegment<byte> compressed, MemoryStream decompressed)
    {
        DeflateStream zlib = this.CompressionLevel == GatewayCompressionLevel.Stream
            ? this.DecompressorStream
            : new DeflateStream(this.CompressedStream, CompressionMode.Decompress, true);

        if (compressed.Array[0] == ZlibPrefix)
        {
            this.CompressedStream.Write(compressed.Array, compressed.Offset + 2, compressed.Count - 2);
        }
        else
        {
            this.CompressedStream.Write(compressed.Array, compressed.Offset, compressed.Count);
        }

        this.CompressedStream.Flush();
        this.CompressedStream.Position = 0;

        Span<byte> cspan = compressed.AsSpan();
        uint suffix = BinaryPrimitives.ReadUInt32BigEndian(cspan.Slice(cspan.Length - 4));
        if (this.CompressionLevel == GatewayCompressionLevel.Stream && suffix != ZlibFlush)
        {
            if (this.CompressionLevel == GatewayCompressionLevel.Payload)
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
            this.CompressedStream.Position = 0;
            this.CompressedStream.SetLength(0);

            if (this.CompressionLevel == GatewayCompressionLevel.Payload)
            {
                zlib.Dispose();
            }
        }
    }

    public void Dispose()
    {
        this.DecompressorStream?.Dispose();
        this.CompressedStream.Dispose();
    }
}
