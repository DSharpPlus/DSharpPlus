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
    public static IReadOnlyDictionary<string, EncryptionMode> SupportedModes { get; }

    public static int NonceSize => Interop.SodiumNonceSize;

    private RandomNumberGenerator CSPRNG { get; }
    private byte[] Buffer { get; }
    private ReadOnlyMemory<byte> Key { get; }

    static Sodium() => SupportedModes = new ReadOnlyDictionary<string, EncryptionMode>(new Dictionary<string, EncryptionMode>()
    {
        ["xsalsa20_poly1305_lite"] = EncryptionMode.XSalsa20_Poly1305_Lite,
        ["xsalsa20_poly1305_suffix"] = EncryptionMode.XSalsa20_Poly1305_Suffix,
        ["xsalsa20_poly1305"] = EncryptionMode.XSalsa20_Poly1305
    });

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
        Helpers.ZeroFill(target[rtpHeader.Length..]);
    }

    public void GenerateNonce(Span<byte> target)
    {
        if (target.Length != Interop.SodiumNonceSize)
        {
            throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));
        }

        this.CSPRNG.GetBytes(this.Buffer);
        this.Buffer.AsSpan().CopyTo(target);
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
        Helpers.ZeroFill(target[4..]);
    }

    public static void AppendNonce(ReadOnlySpan<byte> nonce, Span<byte> target, EncryptionMode encryptionMode)
    {
        switch (encryptionMode)
        {
            case EncryptionMode.XSalsa20_Poly1305:
                return;

            case EncryptionMode.XSalsa20_Poly1305_Suffix:
                nonce.CopyTo(target[^12..]);
                return;

            case EncryptionMode.XSalsa20_Poly1305_Lite:
                nonce[..4].CopyTo(target[^4..]);
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
            case EncryptionMode.XSalsa20_Poly1305:
                source[..12].CopyTo(target);
                return;

            case EncryptionMode.XSalsa20_Poly1305_Suffix:
                source[^Interop.SodiumNonceSize..].CopyTo(target);
                return;

            case EncryptionMode.XSalsa20_Poly1305_Lite:
                source[^4..].CopyTo(target);
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

        int result;
        if ((result = Interop.Encrypt(source, target, this.Key.Span, nonce)) != 0)
        {
            throw new CryptographicException($"Could not encrypt the buffer. Sodium returned code {result}.");
        }
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
    /// The nonce is an incrementing uint32 value. It is encoded as big endian value at the beginning of the nonce buffer. The 4 bytes are also appended at the end of the packet.
    /// </summary>
    XSalsa20_Poly1305_Lite,

    /// <summary>
    /// The nonce consists of random bytes. It is appended at the end of a packet.
    /// </summary>
    XSalsa20_Poly1305_Suffix,

    /// <summary>
    /// The nonce consists of the RTP header. Nothing is appended to the packet.
    /// </summary>
    XSalsa20_Poly1305
}
