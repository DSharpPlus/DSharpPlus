using System;
using System.IO;
using System.IO.Compression;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

internal readonly struct ManagedZlibBackend : IDisposable
{
    private readonly DeflateStream stream;
    private readonly MemoryStream memoryStream;

    public ManagedZlibBackend()
    {
        this.memoryStream = new();
        this.stream = new(this.memoryStream, CompressionMode.Decompress);
    }

    public readonly void Dispose() 
        => this.stream.Dispose();

    public readonly bool Inflate(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        this.memoryStream.Position = 0;
        this.memoryStream.SetLength(0);

        if (compressed[0] == 0x78)
        {
            this.memoryStream.Write(compressed[2..]);
        }
        else
        {
            this.memoryStream.Write(compressed);
        }

        this.memoryStream.Flush();
        this.memoryStream.Position = 0;

        int readLength;

        try
        {
            do
            {
                readLength = this.stream.Read(decompressed.GetSpan());
                decompressed.Advance(readLength);
            } while (readLength != 0);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
