namespace DSharpPlus.Exceptions;
using System.Net.Http;
using System.Text.Json;

/// <summary>
/// Represents an exception thrown when too many requests are sent.
/// </summary>
public class RateLimitException : DiscordException
{
    internal RateLimitException(HttpRequestMessage request, HttpResponseMessage response, string content)
        : base("Rate limited: " + response.StatusCode)
    {
        Request = request;
        Response = response;

        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            JsonElement responseModel = document.RootElement;

            if
            (
                responseModel.TryGetProperty("message", out JsonElement message)
                && message.ValueKind == JsonValueKind.String
            )
            {
                JsonMessage = message.GetString();
            }
        }
        catch { }
    }
}
