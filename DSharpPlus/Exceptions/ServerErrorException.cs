using System;
using DSharpPlus.Net;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when Discord returns an Internal Server Error.
/// </summary>
public class ServerErrorException : DiscordException
{
    internal ServerErrorException(BaseRestRequest request, RestResponse response) : base("Internal Server Error: " + response.ResponseCode)
    {
        this.WebRequest = request;
        this.WebResponse = response;
        if (JObject.Parse(response.Response).TryGetValue("message", StringComparison.Ordinal, out JToken? message))
        {
            this.JsonMessage = message.ToString();
        }
    }
}
