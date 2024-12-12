using System;
using System.IO;
using System.IO.Compression;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A thin wrapper around zlib natives to provide decompression.
/// </summary>
internal readonly struct ZlibWrapper : IDisposable
{
    private readonly ZLibStream stream;
    private readonly MemoryStream underlying;

    public ZlibWrapper()
    {
        this.underlying = new();
        this.stream = new(this.underlying, CompressionMode.Decompress, true);
    }

    /// <summary>
    /// Inflates the provided data.
    /// </summary>
    /// <param name="compressed">The compressed input data.</param>
    /// <param name="decompressed">A span for decompressed data.</param>
    public bool TryInflate(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        try
        {
            this.underlying.Write(compressed);

            this.underlying.Flush();
            this.underlying.Position = 0;

            while (true)
            {
                int read = this.stream.Read(decompressed.GetSpan());

                if (read == 0)
                {
                    break;
                }

                decompressed.Advance(read);
            }

            return true;
        }
        catch (AccessViolationException)
        {
            return false;
        }
        finally
        {
            this.underlying.SetLength(0);
        }
    }

    public void Dispose()
    {
        this.stream.Dispose();
        this.underlying.Dispose();
    }
}
