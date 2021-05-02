using System;
using DSharpPlus.Net;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when the request sent to Discord is too large.
    /// </summary>
    public class RequestSizeException : Exception
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

        internal RequestSizeException(BaseRestRequest request, RestResponse response) : base($"Request entity too large: {response.ResponseCode}. Make sure the data sent is within Discord's upload limit.")
        {
            this.WebRequest = request;
            this.WebResponse = response;

            try
            {
                var j = JObject.Parse(response.Response);

                if (j["message"] != null)
                    this.JsonMessage = j["message"].ToString();
            }
            catch (Exception) { }
        }
    }
}
