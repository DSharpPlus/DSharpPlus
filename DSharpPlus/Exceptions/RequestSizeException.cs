using System.Net.Http;
using System.Text.Json;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when the request sent to Discord is too large.
/// </summary>
public class RequestSizeException : DiscordException
{
    internal RequestSizeException(HttpRequestMessage request, HttpResponseMessage response, string content)
        : base($"Request entity too large: {response.StatusCode}. Make sure the data sent is within Discord's upload limit.")
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
