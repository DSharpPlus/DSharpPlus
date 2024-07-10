using System;
using System.Diagnostics;

namespace DSharpPlus.Net.Gateway.Compression.Zstd;

internal unsafe partial struct ZstdInterop : IDisposable
{
    // must be stored as a pointer. we don't enlighten managed code about the exact layout of this, and we don't
    // really want to.
    private readonly ZstdStream* stream;
    private bool isCompleted = true;
    private int index = 0;

    public ZstdInterop()
    {
        this.stream = Bindings.ZSTD_createDStream();

        nuint code = Bindings.ZSTD_initDStream(this.stream);
        Debug.Assert(Bindings.ZSTD_getErrorCode(code) == ZstdErrorCode.NoError);
    }

    public bool Decompress(ReadOnlySpan<byte> compressed, Span<byte> decompressed, out int written)
    {
        this.isCompleted = false;
        written = 0;

        fixed (byte* pCompressed = compressed)
        fixed (byte* pDecompressed = decompressed)
        {
            while (true)
            {
                ZstdInputBuffer inputBuffer = new()
                {
                    source = pCompressed,
                    position = (nuint)this.index,
                    size = (nuint)compressed.Length
                };

                ZstdOutputBuffer outputBuffer = new()
                {
                    destination = pDecompressed,
                    position = (nuint)written,
                    size = (nuint)decompressed.Length
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

                    written = (int)outputBuffer.position;
                    this.index = (int)inputBuffer.position;
                }
                else if (errorCode is ZstdErrorCode.DestinationSizeTooSmall or ZstdErrorCode.DestinationFull)
                {
                    written = (int)outputBuffer.position;
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
    public static void ThrowZstdError(ZstdErrorCode error)
    {

    }
}
