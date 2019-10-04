﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
#if !NETSTANDARD1_1
using System.Security.Cryptography;
#endif

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class Sodium : IDisposable
    {
        public static IReadOnlyDictionary<string, EncryptionMode> SupportedModes { get; }

        public static int NonceSize => Interop.SodiumNonceSize;

#if !NETSTANDARD1_1
        private RandomNumberGenerator CSPRNG { get; }
        private byte[] Buffer { get; }
#endif
        private ReadOnlyMemory<byte> Key { get; }

        static Sodium()
        {
            SupportedModes = new ReadOnlyDictionary<string, EncryptionMode>(new Dictionary<string, EncryptionMode>()
            {
                ["xsalsa20_poly1305_lite"] = EncryptionMode.XSalsa20_Poly1305_Lite,
#if !NETSTANDARD1_1
                ["xsalsa20_poly1305_suffix"] = EncryptionMode.XSalsa20_Poly1305_Suffix,
#endif
                ["xsalsa20_poly1305"] = EncryptionMode.XSalsa20_Poly1305
            });
        }

        public Sodium(ReadOnlyMemory<byte> key)
        {
            if (key.Length != Interop.SodiumKeySize)
                throw new ArgumentException($"Invalid Sodium key size. Key needs to have a length of {Interop.SodiumKeySize} bytes.", nameof(key));

            this.Key = key;

#if !NETSTANDARD1_1
            this.CSPRNG = RandomNumberGenerator.Create();
            this.Buffer = new byte[Interop.SodiumNonceSize];
#endif
        }

        public void GenerateNonce(ReadOnlySpan<byte> rtpHeader, Span<byte> target)
        {
            if (rtpHeader.Length != Rtp.HeaderSize)
                throw new ArgumentException($"RTP header needs to have a length of exactly {Rtp.HeaderSize} bytes.", nameof(rtpHeader));

            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            // Write the header to the beginning of the span.
            rtpHeader.CopyTo(target);

            // Zero rest of the span.
            Helpers.ZeroFill(target.Slice(rtpHeader.Length));
        }

#if !NETSTANDARD1_1
        public void GenerateNonce(Span<byte> target)
        {
            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            this.CSPRNG.GetBytes(this.Buffer);
            this.Buffer.AsSpan().CopyTo(target);
        }
#endif

        public void GenerateNonce(uint nonce, Span<byte> target)
        {
            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            // Write the uint to memory
            BinaryPrimitives.WriteUInt32BigEndian(target, nonce);

            // Zero rest of the buffer.
            Helpers.ZeroFill(target.Slice(4));
        }

        public void AppendNonce(ReadOnlySpan<byte> nonce, Span<byte> target, EncryptionMode encryptionMode)
        {
            switch (encryptionMode)
            {
                case EncryptionMode.XSalsa20_Poly1305:
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Suffix:
                    nonce.CopyTo(target.Slice(target.Length - 12));
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Lite:
                    nonce.Slice(0, 4).CopyTo(target.Slice(target.Length - 4));
                    return;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
            }
        }

#if !NETSTANDARD1_1
        public void GetNonce(ReadOnlySpan<byte> source, Span<byte> target, EncryptionMode encryptionMode)
        {
            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            switch (encryptionMode)
            {
                case EncryptionMode.XSalsa20_Poly1305:
                    source.Slice(0, 12).CopyTo(target);
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Suffix:
                    source.Slice(source.Length - Interop.SodiumNonceSize).CopyTo(target);
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Lite:
                    source.Slice(source.Length - 4).CopyTo(target);
                    return;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
            }
        }
#endif

        public void Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
        {
            if (nonce.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(nonce));

            if (target.Length != Interop.SodiumMacSize + source.Length)
                throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is a sum of input buffer length and Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(target));

            var result = 0;
            if ((result = Interop.Encrypt(source, target, this.Key.Span, nonce)) != 0)
#if !NETSTANDARD1_1
                throw new CryptographicException(
#else
                throw new Exception(
#endif
                    $"Could not encrypt the buffer. Sodium returned code {result}."
                );
        }

#if !NETSTANDARD1_1
        public void Decrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
        {
            if (nonce.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(nonce));

            if (target.Length != source.Length - Interop.SodiumMacSize)
                throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is input buffer decreased by Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(target));

            var result = 0;
            if ((result = Interop.Decrypt(source, target, this.Key.Span, nonce)) != 0)
#if !NETSTANDARD1_1
                throw new CryptographicException(
#else
                throw new Exception(
#endif
                    $"Could not decrypt the buffer. Sodium returned code {result}."
                );
        }
#endif

        public void Dispose()
        {
#if !NETSTANDARD1_1
            this.CSPRNG.Dispose();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyValuePair<string, EncryptionMode> SelectMode(IEnumerable<string> availableModes)
        {
            foreach (var kvMode in SupportedModes)
                if (availableModes.Contains(kvMode.Key))
                    return kvMode;

#if !NETSTANDARD1_1
            throw new CryptographicException(
#else
            throw new Exception(
#endif
                "Could not negotiate Sodium encryption modes, as none of the modes offered by Discord are supported. This is usually an indicator that something went very wrong."
            );
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
}
