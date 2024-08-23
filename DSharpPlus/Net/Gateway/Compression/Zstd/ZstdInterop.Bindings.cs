using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ZstdErrorCodeConvertible = nuint;

namespace DSharpPlus.Net.Gateway.Compression.Zstd;

internal unsafe partial struct ZstdInterop
{
    // ZstdInterop.Bindings is a nested type to lazily load zstd. the native load is done by the static constructor,
    // which will not be executed unless this code actually gets used. since we cannot rely on zstd being present at all
    // times, it is imperative this remains a nested type.
    private static partial class Bindings
    {
        [LibraryImport("libzstd")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static unsafe partial ZstdStream* ZSTD_createDStream();

        [LibraryImport("libzstd")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static unsafe partial ZstdErrorCodeConvertible ZSTD_freeDStream(ZstdStream* stream);

        [LibraryImport("libzstd")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static unsafe partial ZstdErrorCodeConvertible ZSTD_initDStream(ZstdStream* stream);

        [LibraryImport("libzstd")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static unsafe partial ZstdErrorCodeConvertible ZSTD_decompressStream
        (
            ZstdStream* stream,
            ZstdOutputBuffer* output,
            ZstdInputBuffer* input
        );

        [LibraryImport("libzstd")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static unsafe partial nuint ZSTD_DStreamOutSize();

        [LibraryImport("libzstd")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static unsafe partial ZstdErrorCode ZSTD_getErrorCode(ZstdErrorCodeConvertible returnCode);
    }

    // exists purely to put a name on the relevant parameters
    internal struct ZstdStream;
}
