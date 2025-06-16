using System;
using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop;

internal static partial class OpenSslVersionCheck
{
    public static bool CheckOpenSslVersionCompatible()
    {
        if (OperatingSystem.IsWindows())
        {
            // we provide openssl on windows ourselves
            return true;
        }

        return Bindings.OPENSSL_version_major() >= 3;
    }

    private static partial class Bindings
    {
        /// <summary>
        /// <![CDATA[unsigned int OPENSSL_version_major(void);]]>
        /// </summary>
        [LibraryImport("libcrypto")]
        internal static partial uint OPENSSL_version_major();
    }
}
