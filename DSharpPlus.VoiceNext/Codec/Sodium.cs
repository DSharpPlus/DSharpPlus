using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DSharpPlus.VoiceNext.Codec;

internal sealed class Sodium : IDisposable
{
    public static IReadOnlyDictionary<string, EncryptionMode> SupportedModes { get; } = new ReadOnlyDictionary<string, EncryptionMode>(new Dictionary<string, EncryptionMode>()
    {
        ["aead_aes256_gcm_rtpsize"] = EncryptionMode.AeadAes256GcmRtpSize,
    });

    public static int NonceSize => Interop.SodiumNonceSize;

    private RandomNumberGenerator CSPRNG { get; }
    private byte[] Buffer { get; }
    private ReadOnlyMemory<byte> Key { get; }

    public Sodium(ReadOnlyMemory<byte> key)
    {
        if (key.Length != Interop.SodiumKeySize)
        {
            throw new ArgumentException($"Invalid Sodium key size. Key needs to have a length of {Interop.SodiumKeySize} bytes.", nameof(key));
        }

        this.Key = key;

        this.CSPRNG = RandomNumberGenerator.Create();
        this.Buffer = new byte[Interop.SodiumNonceSize];
    }

    public static void GenerateNonce(ReadOnlySpan<byte> rtpHeader, Span<byte> target)
    {
        if (rtpHeader.Length != Rtp.HeaderSize)
        {
            throw new ArgumentException($"RTP header needs to have a length of exactly {Rtp.HeaderSize} bytes.", nameof(rtpHeader));
        }

        if (target.Length != Interop.SodiumNonceSize)
        {
            throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));
        }

        // Write the header to the beginning of the span.
        rtpHeader.CopyTo(target);

        // Zero rest of the span.
        target[rtpHeader.Length..].Clear();
    }

    public static void GenerateNonce(uint nonce, Span<byte> target)
    {
        if (target.Length != Interop.SodiumNonceSize)
        {
            throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));
        }

        // Write the uint to memory
        BinaryPrimitives.WriteUInt32BigEndian(target, nonce);

        // Zero rest of the buffer.
        target[4..].Clear();
    }

    public static void AppendNonce(ReadOnlySpan<byte> nonce, Span<byte> target, EncryptionMode encryptionMode)
    {
        switch (encryptionMode)
        {
            case EncryptionMode.AeadAes256GcmRtpSize:
                nonce[..4].CopyTo(target[^12..]);
                target[^8..].Clear();
                return;

            default:
                throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
        }
    }

    public static void GetNonce(ReadOnlySpan<byte> source, Span<byte> target, EncryptionMode encryptionMode)
    {
        if (target.Length != Interop.SodiumNonceSize)
        {
            throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));
        }

        switch (encryptionMode)
        {
            case EncryptionMode.AeadAes256GcmRtpSize:
                source[^12..].CopyTo(target);
                return;

            default:
                throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
        }
    }

    public void Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
    {
        if (nonce.Length != Interop.SodiumNonceSize)
        {
            throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(nonce));
        }

        if (target.Length != Interop.SodiumMacSize + source.Length)
        {
            throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is a sum of input buffer length and Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(target));
        }

        Interop.Encrypt(source, target, this.Key.Span, nonce);
    }

    public void Decrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
    {
        if (nonce.Length != Interop.SodiumNonceSize)
        {
            throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(nonce));
        }

        if (target.Length != source.Length - Interop.SodiumMacSize)
        {
            throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is input buffer decreased by Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(target));
        }

        int result;
        if ((result = Interop.Decrypt(source, target, this.Key.Span, nonce)) != 0)
        {
            throw new CryptographicException($"Could not decrypt the buffer. Sodium returned code {result}.");
        }
    }

    public void Dispose() => this.CSPRNG.Dispose();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyValuePair<string, EncryptionMode> SelectMode(IEnumerable<string> availableModes)
    {
        foreach (KeyValuePair<string, EncryptionMode> kvMode in SupportedModes)
        {
            if (availableModes.Contains(kvMode.Key))
            {
                return kvMode;
            }
        }

        throw new CryptographicException("Could not negotiate Sodium encryption modes, as none of the modes offered by Discord are supported. This is usually an indicator that something went very wrong.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateTargetSize(ReadOnlySpan<byte> source)
        => source.Length + Interop.SodiumMacSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateSourceSize(ReadOnlySpan<byte> source)
        => source.Length - Interop.SodiumMacSize;
}

/// <summary>
/// Specifies an encryption mode to use with Sodium.
/// </summary>
public enum EncryptionMode
{
    /// <summary>
    /// The only currently supported encryption mode. Uses a 32-bit incremental nonce.
    /// </summary>
    AeadAes256GcmRtpSize
}
