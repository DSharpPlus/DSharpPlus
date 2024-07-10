using System;
using System.Diagnostics;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A thin wrapper around zlib natives to provide decompression.
/// </summary>
internal unsafe partial struct ZlibInterop : IDisposable
{
    private ZlibStream stream;
    private uint index;

    public ZlibInterop()
    {
        fixed (ZlibStream* pStream = &this.stream)
        {
            ZlibErrorCode code = Bindings.CompressionNative_InflateInit2_(pStream, 15);
            Debug.Assert(code == ZlibErrorCode.Ok);
        }
    }

    /// <summary>
    /// Inflates the provided data.
    /// </summary>
    /// <param name="compressed">The compressed input data.</param>
    /// <param name="decompressed">A span for decompressed data.</param>
    /// <param name="written">The amount of bytes written to the output buffer.</param>
    /// <returns>True if all data was decompressed, false if another call with another output buffer is needed.</returns>
    public bool Inflate(ReadOnlySpan<byte> compressed, Span<byte> decompressed, out int written)
    {
        bool isCompleted = false;

        fixed (byte* pCompressed = compressed)
        fixed (byte* pDecompressed = decompressed)
        fixed (ZlibStream* pStream = &this.stream)
        {
            this.stream.nextInputByte = pCompressed + this.index;
            this.stream.availableInputBytes = (uint)compressed.Length;
            this.stream.nextOutputByte = pDecompressed;
            this.stream.availableOutputBytes = (uint)decompressed.Length;

            while (true)
            {
                ZlibErrorCode code = Bindings.CompressionNative_Inflate(pStream, ZlibFlushCode.SyncFlush);

                if (code == ZlibErrorCode.StreamEnd)
                {
                    isCompleted = true;
                    this.index = 0;
                    break;
                }
                else if (code == ZlibErrorCode.BufferError)
                {
                    // save where we left off for the next call with another output buffer
                    this.index = (uint)compressed.Length - this.stream.availableInputBytes;
                    break;
                }
                else if (code == ZlibErrorCode.Ok)
                {
                    continue;
                }
                else
                {
                    this.index = 0;
                    ThrowHelper.ThrowZlibError(code);
                }
            }

            written = decompressed.Length - (int)this.stream.availableOutputBytes;
        }

        return isCompleted;
    }

    public void Dispose()
    {
        fixed (ZlibStream* pStream = &this.stream)
        {
            Bindings.CompressionNative_InflateEnd(pStream);
        }
    }
}

static file class ThrowHelper
{
    public static void ThrowZlibError(ZlibErrorCode error)
    {

    }
}
