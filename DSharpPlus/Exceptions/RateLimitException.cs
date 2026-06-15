using System;
using System.Net.Http;
using System.Text.Json;

using DSharpPlus.Net;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when too many requests are sent.
/// </summary>
public class RateLimitException : DiscordException
{
    /// <summary>
    /// Specifies how long to wait until this ratelimit is lifted. Note that other requests may cause a repeat to be ratelimited again.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Indicates whether the ratelimit is specific to this request or affects your entire bot.
    /// </summary>
    public bool IsGlobal { get; }

    internal RateLimitException(HttpRequestMessage request, RestResponse response)
        : base("Rate limited: " + response.ResponseCode)
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

            if (responseModel.TryGetProperty("retry_after", out JsonElement retryAfter) && retryAfter.ValueKind == JsonValueKind.Number)
            {
                this.RetryAfter = TimeSpan.FromSeconds(retryAfter.GetSingle());
            }

            if (responseModel.TryGetProperty("global", out JsonElement global) && global.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                this.IsGlobal = global.GetBoolean();
            }
        }
        catch { }
    }
}
