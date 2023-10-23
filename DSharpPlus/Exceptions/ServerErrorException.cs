using System.Net.Http;
using System.Text.Json;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when Discord returns an Internal Server Error.
/// </summary>
public class ServerErrorException : DiscordException
{
    internal ServerErrorException(HttpRequestMessage request, HttpResponseMessage response, string content)
        : base("Internal Server Error: " + response.StatusCode)
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
