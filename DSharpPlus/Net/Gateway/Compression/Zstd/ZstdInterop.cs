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
    private int index = 0;

    internal static int RecommendedBufferSize { get; } = (int)Bindings.ZSTD_DStreamOutSize();

    public ZstdInterop()
    {
        this.stream = Bindings.ZSTD_createDStream();

        nuint code = Bindings.ZSTD_initDStream(this.stream);
        Debug.Assert(Bindings.ZSTD_getErrorCode(code) == ZstdErrorCode.NoError);
    }

    public bool Decompress(ReadOnlySpan<byte> compressed, ArrayPoolBufferWriter<byte> decompressed)
    {
        this.isCompleted = false;

        fixed (byte* pCompressed = compressed)
        {
            while (true)
            {
                ZstdInputBuffer inputBuffer = new()
                {
                    source = pCompressed,
                    position = (nuint)this.index,
                    size = (nuint)compressed.Length
                };

                Span<byte> buffer = decompressed.GetSpan();

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

                    if (errorCode == ZstdErrorCode.NoError)
                    {
                        if (inputBuffer.position == inputBuffer.size)
                        {
                            this.index = 0;
                            this.isCompleted = true;
                            break;
                        }

                        decompressed.Advance((int)outputBuffer.position);
                        this.index = (int)inputBuffer.position;
                    }
                    else if (errorCode is ZstdErrorCode.DestinationSizeTooSmall or ZstdErrorCode.DestinationFull)
                    {
                        decompressed.Advance((int)outputBuffer.position);
                        this.index = (int)inputBuffer.position;
                        break;
                    }
                    else if
                    (
                        errorCode is ZstdErrorCode.Generic or ZstdErrorCode.VersionUnsupported or ZstdErrorCode.SourceEmpty
                    )
                    {
                        ThrowHelper.ThrowZstdError(errorCode);
                    }
                    else
                    {
                        Debug.Assert(true, $"Hit zstd error code {errorCode}");
                        ThrowHelper.ThrowZstdError(errorCode);
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
}
