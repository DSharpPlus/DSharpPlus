using System.Net.Http;
using System.Text.Json;

using DSharpPlus.Net;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when requester doesn't have necessary permissions to complete the request.
/// </summary>
public class UnauthorizedException : DiscordException
{
    internal UnauthorizedException(HttpRequestMessage request, RestResponse response)
        : base("Unauthorized: " + response.ResponseCode)
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
