using System;
using DSharpPlus.Net;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when requester doesn't have necessary permissions to complete the request.
/// </summary>
public class UnauthorizedException : DiscordException
{
    internal UnauthorizedException(BaseRestRequest request, RestResponse response) : base("Unauthorized: " + response.ResponseCode)
    {
        this.WebRequest = request;
        this.WebResponse = response;
        if (JObject.Parse(response.Response).TryGetValue("message", StringComparison.Ordinal, out JToken? message))
        {
            this.JsonMessage = message.ToString();
        }
    }
}
