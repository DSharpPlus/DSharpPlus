using System;
using DSharpPlus.Net;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Represents an exception thrown when the request sent to Discord is too large.
/// </summary>
public class RequestSizeException : DiscordException
{
    internal RequestSizeException(BaseRestRequest request, RestResponse response) : base($"Request entity too large: {response.ResponseCode}. Make sure the data sent is within Discord's upload limit")
    {
        this.WebRequest = request;
        this.WebResponse = response;
        if (JObject.Parse(response.Response).TryGetValue("message", StringComparison.Ordinal, out JToken? message))
        {
            this.JsonMessage = message.ToString();
        }
    }
}
