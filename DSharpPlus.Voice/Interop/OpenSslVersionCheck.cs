using System;
using System.Runtime.InteropServices;

namespace DSharpPlus.Voice.Interop;

internal static partial class OpenSslVersionCheck
{
    public static bool CheckOpenSslVersionCompatible()
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            // we provide openssl on windows and macos ourselves
            return true;
        }

        return Bindings.OPENSSL_version_major() >= 3;
    }

    private static partial class Bindings
    {
        /// <summary>
        /// <code>
        /// <![CDATA[unsigned int OPENSSL_version_major(void);]]>
        /// </code>
        /// </summary>
        [LibraryImport("libcrypto")]
        internal static partial uint OPENSSL_version_major();
    }
}
