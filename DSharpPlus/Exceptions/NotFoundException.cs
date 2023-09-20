using System;
using DSharpPlus.Net;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : DiscordException
{
    internal NotFoundException(BaseRestRequest request, RestResponse response) : base("Not found: " + response.ResponseCode)
    {
        this.WebRequest = request;
        this.WebResponse = response;
        if (JObject.Parse(response.Response).TryGetValue("message", StringComparison.Ordinal, out JToken? message))
        {
            this.JsonMessage = message.ToString();
        }
    }
}
