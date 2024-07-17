using System;
using System.Runtime.InteropServices;

namespace DSharpPlus.Net.HttpInteractions;

public static partial class Ed25519
{
    public const int SignatureBytes = 64;
    public const int PublicKeyBytes = 32;
    
    public static unsafe bool TryVerifySignature(ReadOnlySpan<byte> body, ReadOnlySpan<byte> publicKey, ReadOnlySpan<byte> signature)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(signature.Length, SignatureBytes);
        ArgumentOutOfRangeException.ThrowIfNotEqual(publicKey.Length, PublicKeyBytes);
        
        fixed (byte* signaturePtr = signature)
        fixed (byte* messagePtr = body)
        fixed (byte* publicKeyPtr = publicKey)
        {
            return Bindings.crypto_sign_ed25519_verify_detached(signaturePtr, messagePtr, (ulong)body.Length, publicKeyPtr) == 0;
        }
    }

    // Ed25519.Bindings is a nested type to lazily load sodium. the native load is done by the static constructor,
    // which will not be executed unless this code actually gets used. since we cannot rely on sodium being present at all
    // times, it is imperative this remains a nested type.
    private static partial class Bindings
    {
        static Bindings()
        {
            if (sodium_init() == -1)
            {
                throw new InvalidOperationException("Failed to initialize libsodium.");
            }
        }
        
        [LibraryImport("sodium")]
        private static unsafe partial int sodium_init();

        [LibraryImport("sodium")]
        internal static unsafe partial int crypto_sign_ed25519_verify_detached(byte* signature, byte* message, ulong messageLength, byte* publicKey);
    }
}
