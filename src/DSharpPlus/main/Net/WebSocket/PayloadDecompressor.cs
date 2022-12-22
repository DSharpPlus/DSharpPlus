// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
        uint suffix = BinaryPrimitives.ReadUInt32BigEndian(cspan.Slice(cspan.Length - 4));
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
