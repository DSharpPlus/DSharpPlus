﻿// Parts of the code adapted from:
// https://github.com/adamcaudill/libsodium-net/blob/master/libsodium-net/SecretBox.cs
// https://github.com/adamcaudill/libsodium-net/blob/master/libsodium-net/SodiumLibrary.cs

using System;
using System.Runtime.InteropServices;
#if !NETSTANDARD1_1
using System.Security.Cryptography;
#endif

namespace DSharpPlus.VoiceNext.Codec
{
    internal sealed class SodiumCodec
    {
        private const int KeyBytes = 32;
        private const int NonceBytes = 24;
        private const int MacBytes = 16;

        [DllImport("libsodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
        public static extern int CreateSecretBox(byte[] buffer, byte[] message, long messageLength, byte[] nonce, byte[] key);

#if !NETSTANDARD1_1
        [DllImport("libsodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_open_easy")]
        public static extern int OpenSecretBox(byte[] buffer, byte[] message, long messageLength, byte[] nonce, byte[] key);
#endif

        public byte[] Encode(byte[] input, byte[] nonce, byte[] secretKey)
        {
            // SecretBox.Create(input, nonce, secret_key);

            if (secretKey == null || secretKey.Length != KeyBytes)
            {
                throw new ArgumentException("Invalid key.");
            }

            if (nonce == null || nonce.Length != NonceBytes)
            {
                throw new ArgumentException("Invalid nonce.");
            }

            var buff = new byte[MacBytes + input.Length];
            var err = CreateSecretBox(buff, input, input.Length, nonce, secretKey);

            if (err != 0)
            {
#if NETSTANDARD1_1
                throw new Exception("Error encrypting data.");
#else
                throw new CryptographicException("Error encrypting data.");
#endif
            }

            return buff;
        }
        
#if !NETSTANDARD1_1
        public byte[] Decode(byte[] input, byte[] nonce, byte[] secretKey)
        {
            if (secretKey == null || secretKey.Length != KeyBytes)
            {
                throw new ArgumentException("Invalid key.");
            }

            if (nonce == null || nonce.Length != NonceBytes)
            {
                throw new ArgumentException("Invalid nonce.");
            }

            var buff = new byte[input.Length - MacBytes];
            var err = OpenSecretBox(buff, input, input.Length, nonce, secretKey);

            if (err != 0)
            {
                throw new CryptographicException("Error decrypting data.");
            }

            return buff;
        }
#endif
    }
}
