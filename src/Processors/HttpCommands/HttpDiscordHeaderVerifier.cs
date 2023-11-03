using System;
using System.Net;
using System.Text;

namespace DSharpPlus.CommandAll.Processors.HttpCommands
{
    public static class HttpDiscordHeaderVerifier
    {
        public static bool TryVerify(HttpListenerContext context, byte[] publicKey)
        {
            // Ensure the request is from Discord first. Otherwise malicious input could be sent within the headers.
            string? timestamp = context.Request.Headers.Get("X-Signature-Timestamp");
            string? signature = context.Request.Headers.Get("X-Signature-Ed25519");
            if (timestamp is null || signature is null)
            {
                Respond(context, "Missing authentication headers.");
                return false;
            }

            // Grab the content length, which still could be malicious.
            string? contentLength = context.Request.Headers.Get("Content-Length");
            if (string.IsNullOrEmpty(contentLength) || !int.TryParse(contentLength, out int length))
            {
                Respond(context, "Invalid or incorrect content length.");
                return false;
            }

            // Create an array to hold the message and timestamp.
            int timestampLength = Encoding.UTF8.GetByteCount(timestamp);
            byte[] message = new byte[length + timestampLength];

            // Fill the first part of the message with the timestamp.
            Encoding.UTF8.GetBytes(timestamp, message);

            // Try to read the whole message from the stream, appending to the timestamp.
            // If the content length was incorrect, this will fail safely.
            int read = context.Request.InputStream.ReadAtLeast(message.AsSpan(timestampLength, length), length, false);
            if (read != length)
            {
                Respond(context, "Invalid or incorrect content length.");
                return false;
            }

            // Verify the signature.
            string content = Encoding.UTF8.GetString(message.AsSpan(timestampLength));
            Span<byte> signatureSpan = stackalloc byte[64];
            FromHex(signature, signatureSpan);
            if (!HttpEd25519.Verify(signatureSpan, message, publicKey))
            {
                Respond(context, "Invalid authentication headers.");
                return false;
            }

            return true;
        }

        private static void Respond(HttpListenerContext context, string response)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "text/plain";
            context.Response.Close(Encoding.UTF8.GetBytes(response), false);
        }

        private static void FromHex(ReadOnlySpan<char> hex, Span<byte> destination)
        {
            if ((hex.Length & 1) == 1)
            {
                throw new ArgumentException("Hex string must have an even number of characters.");
            }
            else if (destination.Length < hex.Length / 2)
            {
                throw new ArgumentException("Destination buffer is too small.");
            }

            for (int i = 0, j = 0; i < hex.Length; i += 2, j++)
            {
                byte highNibble = HexCharToByte(hex[i]);
                byte lowNibble = HexCharToByte(hex[i + 1]);
                destination[j] = (byte)((highNibble << 4) | lowNibble);
            }
        }

        private static byte HexCharToByte(char c) => c switch
        {
            >= '0' and <= '9' => (byte)(c - '0'),
            >= 'a' and <= 'f' => (byte)(c - 'a' + 10),
            >= 'A' and <= 'F' => (byte)(c - 'A' + 10),
            _ => throw new ArgumentException($"Invalid hex character '{c}'."),
        };
    }
}
