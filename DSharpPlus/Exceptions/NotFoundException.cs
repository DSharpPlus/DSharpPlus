using System.Net.Http;
using System.Text.Json;

using DSharpPlus.Net;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : DiscordException
{
    internal NotFoundException(HttpRequestMessage request, RestResponse response)
        : base("Not found: " + response.ResponseCode)
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
