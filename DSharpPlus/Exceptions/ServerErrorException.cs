using System.Net.Http;
using System.Text.Json;

using DSharpPlus.Net;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when Discord returns an Internal Server Error.
/// </summary>
public class ServerErrorException : DiscordException
{
    internal ServerErrorException(HttpRequestMessage request, RestResponse response)
        : base("Internal Server Error: " + response.ResponseCode)
    {
        this.Request = request;
        this.Response = response;

        try
        {
            using JsonDocument document = JsonDocument.Parse(response.Response);
            JsonElement responseModel = document.RootElement;

            if (responseModel.TryGetProperty("message", out JsonElement message) && message.ValueKind == JsonValueKind.String)
            {
                this.JsonMessage = message.GetString();
            }
        }
        catch { }
    }
}
