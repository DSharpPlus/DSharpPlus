using Newtonsoft.Json.Linq;
using System;
using DSharpPlus.Net;

namespace DSharpPlus.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a malformed request is sent.
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Gets the request that caused the exception.
        /// </summary>
        public BaseRestRequest WebRequest { get; internal set; }

        /// <summary>
        /// Gets the response to the request.
        /// </summary>
        public RestResponse WebResponse { get; internal set; }

        /// <summary>
        /// Gets the JSON received.
        /// </summary>
        public string JsonMessage { get; internal set; }

        internal BadRequestException(BaseRestRequest request, RestResponse response) : base("Bad request: " + response.ResponseCode)
        {
            this.WebRequest = request;
            this.WebResponse = response;

            try
            {
                JObject j = JObject.Parse(response.Response);

                if (j["message"] != null)
                    JsonMessage = j["message"].ToString();
            }
            catch (Exception) { }
        }
    }
}
