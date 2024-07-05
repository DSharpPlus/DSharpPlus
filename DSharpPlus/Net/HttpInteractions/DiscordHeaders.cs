using System;

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

    public static bool VerifySignature(Span<byte>)
    {
        
    }

}
