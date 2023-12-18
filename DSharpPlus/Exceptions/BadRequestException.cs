using System.Net.Http;
using System.Text.Json;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when a malformed request is sent.
/// </summary>
public class BadRequestException : DiscordException
{

    /// <summary>
    /// Gets the error code for this exception.
    /// </summary>
    public int Code { get; internal set; }

    /// <summary>
    /// Gets the form error responses in JSON format.
    /// </summary>
    public string? Errors { get; internal set; }

    internal BadRequestException(HttpRequestMessage request, HttpResponseMessage response, string content)
        : base("Bad request: " + response.StatusCode)
    {
        this.Request = request;
        this.Response = response;

        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            JsonElement responseModel = document.RootElement;

            if
            (
                responseModel.TryGetProperty("code", out JsonElement code)
                && code.ValueKind == JsonValueKind.Number
            )
            {
                this.Code = code.GetInt32();
            }

            if
            (
                responseModel.TryGetProperty("message", out JsonElement message)
                && message.ValueKind == JsonValueKind.String
            )
            {
                this.JsonMessage = message.GetString();
            }

            if
            (
                responseModel.TryGetProperty("errors", out JsonElement errors)
                && message.ValueKind == JsonValueKind.String
            )
            {
                this.Errors = errors.GetString();
            }
        }
        catch { }
    }
}
