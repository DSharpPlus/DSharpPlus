using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using Microsoft.Extensions.Options;

namespace DSharpPlus.Net.WebSocket;

public sealed class PayloadDecompressor : IDisposable
{
    private const uint ZlibFlush = 0x0000FFFF;
    private const byte ZlibPrefix = 0x78;

    public GatewayCompressionLevel CompressionLevel { get; }

    private MemoryStream CompressedStream { get; }
    private DeflateStream DecompressorStream { get; }

    private readonly bool noCompression;

    public PayloadDecompressor(IOptions<DiscordConfiguration> config)
    {
        this.noCompression = config.Value.GatewayCompressionLevel == GatewayCompressionLevel.None;

        this.CompressionLevel = config.Value.GatewayCompressionLevel;
        this.CompressedStream = new MemoryStream();
        if (this.CompressionLevel == GatewayCompressionLevel.Stream)
        {
            this.DecompressorStream = new DeflateStream(this.CompressedStream, CompressionMode.Decompress);
        }
    }

    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (this.noCompression)
        {
            decompressed.Write(compressed);
            return true;
        }

        DeflateStream zlib = this.CompressionLevel == GatewayCompressionLevel.Stream
            ? this.DecompressorStream
            : new DeflateStream(this.CompressedStream, CompressionMode.Decompress, true);

        if (compressed[0] == ZlibPrefix)
        {
            this.CompressedStream.Write(compressed[2..]);
        }
        else
        {
            this.CompressedStream.Write(compressed);
        }

        this.CompressedStream.Flush();
        this.CompressedStream.Position = 0;

        uint suffix = BinaryPrimitives.ReadUInt32BigEndian(compressed[^4..]);
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
            int readLength;

            do
            {
                readLength = zlib.Read(decompressed.GetSpan());
                decompressed.Advance(readLength);
            } while (readLength != 0);

            return true;
        }
        catch
        {
            return false;
        }
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
