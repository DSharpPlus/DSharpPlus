
using System.Net.Http;
using System.Text.Json;

namespace DSharpPlus.Exceptions;
/// <summary>
/// Represents an exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : DiscordException
{
    internal NotFoundException(HttpRequestMessage request, HttpResponseMessage response, string content)
        : base("Not found: " + response.StatusCode)
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
