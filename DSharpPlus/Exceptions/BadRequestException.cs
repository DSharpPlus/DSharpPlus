using System;
using DSharpPlus.Net;
using Newtonsoft.Json.Linq;

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

    internal BadRequestException(BaseRestRequest request, RestResponse response) : base("Bad request: " + response.ResponseCode)
    {
        this.WebRequest = request;
        this.WebResponse = response;

        JObject jsonResponse = JObject.Parse(response.Response);
        if (jsonResponse.TryGetValue("message", StringComparison.Ordinal, out JToken? message))
        {
            this.JsonMessage = message.ToString();
        }

        if (jsonResponse.TryGetValue("code", StringComparison.Ordinal, out JToken? code))
        {
            this.Code = (int)code;
        }

        if (jsonResponse.TryGetValue("errors", StringComparison.Ordinal, out JToken? errors))
        {
            this.Errors = errors.ToString();
        }
    }
}
