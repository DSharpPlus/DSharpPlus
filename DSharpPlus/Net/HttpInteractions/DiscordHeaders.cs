using System;
using System.Buffers;
using System.Text;

namespace DSharpPlus.Net.HttpInteractions;

public class DiscordHeaders
{
    /// <summary>
    /// Name of the HTTP header which contains the timestamp of the signature
    /// </summary>
    public const string TimestampHeaderName = "x-signature-timestamp";
    
    /// <summary>
    /// Name of the HTTP header which contains the signature
    /// </summary>
    public const string SignatureHeaderName = "x-signature-ed25519";

    public static bool VerifySignature(ReadOnlySpan<byte> body, string timestamp, string signingKey, string publicKey)
    {
        byte[] timestampBytes = Encoding.UTF8.GetBytes(timestamp);
        byte[] publicKeyBytes = Convert.FromHexString(publicKey);
        byte[] signatureBytes = Convert.FromHexString(signingKey);

        byte[] message = ArrayPool<byte>.Shared.Rent(body.Length + timestampBytes.Length);
        
        timestampBytes.CopyTo(message, 0);
        body.CopyTo(message.AsSpan(timestampBytes.Length));

        bool result = Ed25519.TryVerifySignature(message.AsSpan(), publicKeyBytes.AsSpan(), signatureBytes.AsSpan());
        
        ArrayPool<byte>.Shared.Return(message);

        return result;
    }

}
