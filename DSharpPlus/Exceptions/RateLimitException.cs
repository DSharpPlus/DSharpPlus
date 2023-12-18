using System.Net.Http;
using System.Text.Json;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when too many requests are sent.
/// </summary>
public class RateLimitException : DiscordException
{
    internal RateLimitException(HttpRequestMessage request, HttpResponseMessage response, string content)
        : base("Rate limited: " + response.StatusCode)
    {
        this.Request = request;
        this.Response = response;

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
                this.JsonMessage = message.GetString();
            }
        }
        catch { }
    }
}
