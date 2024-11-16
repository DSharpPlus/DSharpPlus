using System;
using System.Buffers;
using System.Text;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.InboundWebhooks;

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

    /// <summary>
    /// Verifies the signature of a http interaction.
    /// </summary>
    /// <param name="body">Raw http body</param>
    /// <param name="timestamp">Timestamp header sent by discord. <see cref="TimestampHeaderName"/></param>
    /// <param name="signingKey">Signing key sent by discord. <see cref="SignatureHeaderName"/></param>
    /// <param name="publicKey">
    /// Public key of the application this interaction was sent.
    /// This key can be accessed at DiscordApplication.
    /// <see cref="DiscordApplication.VerifyKey"/>
    /// </param>
    /// <returns>Indicates if this signature is valid.</returns>
    public static bool VerifySignature(ReadOnlySpan<byte> body, string timestamp, string signingKey, string publicKey)
    {
        byte[] timestampBytes = Encoding.UTF8.GetBytes(timestamp);
        byte[] publicKeyBytes = Convert.FromHexString(publicKey);
        byte[] signatureBytes = Convert.FromHexString(signingKey);

        int messageLength = body.Length + timestampBytes.Length;
        byte[] message = ArrayPool<byte>.Shared.Rent(messageLength);

        timestampBytes.CopyTo(message, 0);
        body.CopyTo(message.AsSpan(timestampBytes.Length));

        bool result = Ed25519.TryVerifySignature(message.AsSpan(..messageLength), publicKeyBytes.AsSpan(), signatureBytes.AsSpan());

        ArrayPool<byte>.Shared.Return(message);

        return result;
    }
}
