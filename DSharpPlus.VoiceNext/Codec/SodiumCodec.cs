using Sodium;

namespace DSharpPlus.VoiceNext.Codec
{
    public sealed class SodiumCodec
    {
        public byte[] Encode(byte[] input, byte[] nonce, byte[] secret_key) =>
            SecretBox.Create(input, nonce, secret_key);
    }
}
