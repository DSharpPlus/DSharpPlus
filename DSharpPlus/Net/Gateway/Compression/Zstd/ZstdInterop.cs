using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Net.Gateway.Compression.Zstd;

internal unsafe partial struct ZstdInterop : IDisposable
{
    // must be stored as a pointer. we don't enlighten managed code about the exact layout of this, and we don't
    // really want to.
    private readonly ZstdStream* stream;
    private bool isCompleted = true;

    internal static int RecommendedBufferSize { get; } = (int)Bindings.ZSTD_DStreamOutSize();

    public ZstdInterop()
    {
        this.stream = Bindings.ZSTD_createDStream();

        nuint code = Bindings.ZSTD_initDStream(this.stream);
        Debug.Assert(Bindings.ZSTD_getErrorCode(code) == ZstdErrorCode.NoError);
    }

    public bool TryDecompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        this.isCompleted = false;

        while (true)
        {
            fixed (byte* pCompressed = compressed)
            {
                ZstdInputBuffer inputBuffer = new()
                {
                    source = pCompressed,
                    position = 0,
                    size = (nuint)compressed.Length
                };

                Span<byte> buffer = decompressed.GetSpan(compressed.Length);

                fixed (byte* pDecompressed = buffer)
                {
                    ZstdOutputBuffer outputBuffer = new()
                    {
                        destination = pDecompressed,
                        position = 0,
                        size = (nuint)buffer.Length
                    };

                    nuint code = Bindings.ZSTD_decompressStream(this.stream, &outputBuffer, &inputBuffer);
                    ZstdErrorCode errorCode = Bindings.ZSTD_getErrorCode(code);

                    decompressed.Advance((int)outputBuffer.position);
                    compressed = compressed[(int)inputBuffer.position..];

                    if (errorCode == ZstdErrorCode.NoError)
                    {
                        if (outputBuffer.position < outputBuffer.size)
                        {
                            this.isCompleted = true;
                            break;
                        }

                        continue;
                    }
                    else if (errorCode is ZstdErrorCode.DestinationSizeTooSmall or ZstdErrorCode.DestinationFull)
                    {
                        continue;
                    }
                    else
                    {
                        Debug.Assert(true, $"Hit zstd error code {errorCode}");
                        return false;
                    }
                }
            }
        }

        return this.isCompleted;
    }

    public void Dispose()
    {
        nuint code = Bindings.ZSTD_freeDStream(this.stream);
        Debug.Assert(Bindings.ZSTD_getErrorCode(code) == ZstdErrorCode.NoError);
    }
}

static file class ThrowHelper
{
    [DoesNotReturn]
    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowZstdError(ZstdErrorCode error) 
        => throw new InvalidDataException($"Encountered an error in deserializing a ZSTD payload: {error}");

    [DoesNotReturn]
    [DebuggerHidden]
    [StackTraceHidden]
    public static int ThrowZstdInvalidHeader()
        => throw new InvalidCastException($"Encountered an invalid ZSTD frame header.");
}
