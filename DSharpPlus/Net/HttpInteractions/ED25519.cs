using System;
using System.Runtime.InteropServices;

namespace DSharpPlus.Net.HttpInteractions;

public static partial class Ed25519
{
    public const int SignatureBytes = 64;
    public const int PublicKeyBytes = 32;

    static Ed25519()
    {
        if (Init() == -1)
        {
            throw new InvalidOperationException("Failed to initialize libsodium.");
        }
    }

    public static unsafe bool TryVerifySignature(ReadOnlySpan<byte> body, ReadOnlySpan<byte> publicKey, ReadOnlySpan<byte> signature)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(signature.Length, SignatureBytes);
        ArgumentOutOfRangeException.ThrowIfNotEqual(publicKey.Length, PublicKeyBytes);
        
        fixed (byte* signaturePtr = signature)
        fixed (byte* messagePtr = body)
        fixed (byte* publicKeyPtr = publicKey)
        {
            return VerifyDetached(signaturePtr, messagePtr, (ulong)body.Length, publicKeyPtr) == 0;
        }
    }

    [LibraryImport("libsodium", EntryPoint = "sodium_init")]
    private static unsafe partial int Init();

    [LibraryImport("libsodium", EntryPoint = "crypto_sign_ed25519_verify_detached")]
    private static unsafe partial int VerifyDetached(byte* signature, byte* message, ulong messageLength, byte* publicKey);
}
