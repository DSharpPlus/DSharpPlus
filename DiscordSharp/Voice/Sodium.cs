using System.Runtime.InteropServices;
#if NETFX4_5
using System.Security;
#endif

namespace DiscordSharp.Voice
{
    internal unsafe static class SecretBox
    {
#if NETFX4_5
        [SuppressUnmanagedCodeSecurity]
#endif
        private static class SafeNativeMethods
        {
            [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
            public static extern int SecretBoxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);
            [DllImport("libsodium", EntryPoint = "crypto_secretbox_open_easy", CallingConvention = CallingConvention.Cdecl)]
            public static extern int SecretBoxOpenEasy(byte[] output, byte* input, ulong inputLength, byte[] nonce, byte[] secret);
        }

        public static int Encrypt(byte[] input, long inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            fixed (byte* outPtr = output)
                return SafeNativeMethods.SecretBoxEasy(outPtr + outputOffset, input, inputLength, nonce, secret);
        }
        public static int Decrypt(byte[] input, int inputOffset, ulong inputLength, byte[] output, byte[] nonce, byte[] secret)
        {
            fixed (byte* inPtr = input)
                return SafeNativeMethods.SecretBoxOpenEasy(output, inPtr + inputOffset, inputLength, nonce, secret);
        }
    }
}
