using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop.Aerith;

#pragma warning disable IDE0040
partial class AerithInterop
#pragma warning restore IDE0040
{
    private static unsafe partial class Bindings
    {
        [LibraryImport("aerith")]
        public static partial void* aerith_create_user_credential(byte* userId);

        [LibraryImport("aerith")]
        public static partial void aerith_free_credential(void* credential);

        [LibraryImport("aerith")]
        public static partial void aerith_deserialize_credential(void* credential, byte* buffer);
    }
}
