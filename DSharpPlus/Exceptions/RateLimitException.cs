using System;
using DSharpPlus.Net;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when too many requests are sent.
/// </summary>
public class RateLimitException : DiscordException
{
    internal RateLimitException(BaseRestRequest request, RestResponse response) : base("Rate limited: " + response.ResponseCode)
    {
        this.WebRequest = request;
        this.WebResponse = response;
        if (JObject.Parse(response.Response).TryGetValue("message", StringComparison.Ordinal, out JToken? message))
        {
            this.JsonMessage = message.ToString();
        }
    }
}
