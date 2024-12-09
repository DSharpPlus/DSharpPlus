using System;
using System.IO.Compression;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

internal readonly struct ManagedZlibBackend : IDisposable
{
    private readonly DeflateStream stream;

    public readonly void Dispose() 
        => this.stream.Dispose();

    public readonly bool Inflate(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        if (compressed[0] == 0x78)
        {
            this.stream.Write(compressed[2..]);
        }
        else
        {
            this.stream.Write(compressed);
        }

        this.stream.Flush();
        this.stream.Position = 0;

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
