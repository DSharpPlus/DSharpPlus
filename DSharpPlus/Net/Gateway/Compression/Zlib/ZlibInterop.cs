using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

/// <summary>
/// A thin wrapper around zlib natives to provide decompression.
/// </summary>
internal unsafe partial struct ZlibInterop : IDisposable
{
    private ZlibStream stream;

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
    /// <returns>True if all data was decompressed, false if another call with another output buffer is needed.</returns>
    public bool Inflate(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        bool isCompleted = false;

        fixed (byte* pCompressed = compressed)
        fixed (ZlibStream* pStream = &this.stream)
        {
            pStream->nextInputByte = pCompressed;
            pStream->availableInputBytes = (uint)compressed.Length;

            while (true)
            {
                uint inputBytes = pStream->availableInputBytes;

                Span<byte> buffer = decompressed.GetSpan(compressed.Length);

                fixed (byte* pDecompressed = buffer)
                {
                    pStream->nextOutputByte = pDecompressed;
                    pStream->availableOutputBytes = (uint)buffer.Length;

                    ZlibErrorCode code = Bindings.CompressionNative_Inflate(pStream, ZlibFlushCode.SyncFlush);
                    decompressed.Advance(buffer.Length - (int)pStream->availableOutputBytes);
                    compressed = compressed[^(int)pStream->availableInputBytes..];

                    if (code == ZlibErrorCode.StreamEnd)
                    {
                        isCompleted = true;
                        break;
                    }
                    else if (code == ZlibErrorCode.BufferError)
                    {
                        continue;
                    }
                    else if (code == ZlibErrorCode.Ok)
                    {
                        if (pStream->availableInputBytes == 0)
                        {
                            isCompleted = true;
                            break;
                        }

                        continue;
                    }
                    else
                    {
                        ThrowHelper.ThrowZlibError(code);
                    }
                }
            }

            Debug.Assert(pStream->availableInputBytes == 0);
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
    [DoesNotReturn]
    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowZlibError(ZlibErrorCode error)
        => throw new InvalidDataException($"Encountered an error in deserializing a zlib payload: {error}");
}
