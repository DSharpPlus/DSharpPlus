using System.Runtime.InteropServices;

namespace DSharpPlus.Net.Gateway.Compression.Zlib;

internal unsafe partial struct RuntimeBundledZlibBackend
{
    // ZlibInterop.Bindings is a nested type to lazily load System.IO.Compression.Native. the native load is done by
    // the static constructor, which will not be executed unless this code actually gets used.
    private static unsafe partial class Bindings
    {
        [LibraryImport("System.IO.Compression.Native")]
        internal static unsafe partial ZlibErrorCode CompressionNative_InflateInit2_(ZlibStream* stream, int windowBits);

        [LibraryImport("System.IO.Compression.Native")]
        internal static unsafe partial ZlibErrorCode CompressionNative_Inflate(ZlibStream* stream, ZlibFlushCode flushCode);

        [LibraryImport("System.IO.Compression.Native")]
        internal static unsafe partial ZlibErrorCode CompressionNative_InflateEnd(ZlibStream* stream);
    }
}
